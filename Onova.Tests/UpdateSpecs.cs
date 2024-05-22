using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using oZnova.Models;
using oZnova.Tests.Utils;
using Xunit;
using Xunit.Abstractions;

namespace oZnova.Tests;

public partial class UpdateSpecs : IDisposable
{
    private readonly ITestOutputHelper _testOutput;

    private string TempDirPath { get; } =
        Path.Combine(Directory.GetCurrentDirectory(), $"{nameof(UpdateSpecs)}_{Guid.NewGuid()}");

    public UpdateSpecs(ITestOutputHelper testOutput)
    {
        _testOutput = testOutput;

        DirectoryEx.Reset(TempDirPath);
    }

    public void Dispose()
    {
        for (var retriesRemaining = 5; ; retriesRemaining--)
        {
            try
            {
                DirectoryEx.DeleteIfExists(TempDirPath);
                break;
            }
            catch (UnauthorizedAccessException) when (retriesRemaining > 0)
            {
                Thread.Sleep(1000);
            }
        }
    }

    [Fact]
    public async Task I_can_check_for_updates_and_get_a_higher_version_if_it_is_available()
    {
        // Arrange
        var updatee = new AssemblyMetadata("TestUpdatee", Version.Parse("1.0"), "");

        // Cleanup storage directory (TODO: move this to API)
        DirectoryEx.DeleteIfExists(
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "oZnova",
                updatee.Name
            )
        );

        var availableVersions = new[]
        {
            Version.Parse("1.0"),
            Version.Parse("2.0"),
            Version.Parse("3.0")
        };

        using var updateManager = new UpdateManager(
            updatee,
            new FakePackageResolver(availableVersions),
            new FakePackageExtractor()
        );

        // Act
        var result = await updateManager.CheckForUpdatesAsync();

        // Assert
        result.CanUpdate.Should().BeTrue();
        result.Versions.Should().BeEquivalentTo(availableVersions);
        result.LastVersion.Should().Be(Version.Parse("3.0"));
    }

    [Fact]
    public async Task I_can_check_for_updates_and_get_nothing_if_there_is_no_higher_version_available()
    {
        // Arrange
        var updatee = new AssemblyMetadata("TestUpdatee", Version.Parse("3.0"), "");

        // Cleanup storage directory (TODO: move this to API)
        DirectoryEx.DeleteIfExists(
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "oZnova",
                updatee.Name
            )
        );

        var availableVersions = new[]
        {
            Version.Parse("1.0"),
            Version.Parse("2.0"),
            Version.Parse("3.0")
        };

        using var updateManager = new UpdateManager(
            updatee,
            new FakePackageResolver(availableVersions),
            new FakePackageExtractor()
        );

        // Act
        var result = await updateManager.CheckForUpdatesAsync();

        // Assert
        result.CanUpdate.Should().BeFalse();
        result.Versions.Should().BeEquivalentTo(availableVersions);
        result.LastVersion.Should().Be(updatee.Version);
    }

    [Fact]
    public async Task I_can_check_for_updates_and_get_nothing_if_the_package_source_contains_no_packages()
    {
        // Arrange
        var updatee = new AssemblyMetadata("TestUpdatee", Version.Parse("1.0"), "");

        // Cleanup storage directory (TODO: move this to API)
        DirectoryEx.DeleteIfExists(
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "oZnova",
                updatee.Name
            )
        );

        var availableVersions = Array.Empty<Version>();

        using var updateManager = new UpdateManager(
            updatee,
            new FakePackageResolver(availableVersions),
            new FakePackageExtractor()
        );

        // Act
        var result = await updateManager.CheckForUpdatesAsync();

        // Assert
        result.CanUpdate.Should().BeFalse();
        result.Versions.Should().BeEmpty();
        result.LastVersion.Should().BeNull();
    }

    [Fact]
    public async Task I_can_prepare_an_update_so_that_it_can_be_installed()
    {
        // Arrange
        var updatee = new AssemblyMetadata("TestUpdatee", Version.Parse("1.0"), "");

        // Cleanup storage directory (TODO: move this to API)
        DirectoryEx.DeleteIfExists(
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "oZnova",
                updatee.Name
            )
        );

        var availableVersions = new[]
        {
            Version.Parse("1.0"),
            Version.Parse("2.0"),
            Version.Parse("3.0")
        };

        using var updateManager = new UpdateManager(
            updatee,
            new FakePackageResolver(availableVersions),
            new FakePackageExtractor()
        );

        var version = Version.Parse("2.0");

        // Act
        await updateManager.PrepareUpdateAsync(version);

        // Assert
        updateManager.IsUpdatePrepared(version).Should().BeTrue();
    }

    [Fact]
    public async Task I_can_get_a_list_of_updates_which_are_already_prepared_to_install()
    {
        // Arrange
        var updatee = new AssemblyMetadata("TestUpdatee", Version.Parse("1.0"), "");

        // Cleanup storage directory (TODO: move this to API)
        DirectoryEx.DeleteIfExists(
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "oZnova",
                updatee.Name
            )
        );

        var availableVersions = new[]
        {
            Version.Parse("1.0"),
            Version.Parse("2.0"),
            Version.Parse("3.0")
        };

        using var manager = new UpdateManager(
            updatee,
            new FakePackageResolver(availableVersions),
            new FakePackageExtractor()
        );

        var expectedPreparedUpdateVersions = new[] { Version.Parse("1.0"), Version.Parse("3.0") };

        foreach (var version in expectedPreparedUpdateVersions)
            await manager.PrepareUpdateAsync(version);

        // Act
        var preparedUpdateVersions = manager.GetPreparedUpdates();

        // Assert
        preparedUpdateVersions.Should().BeEquivalentTo(expectedPreparedUpdateVersions);
    }

    [Fact(Timeout = 10000, Skip = "Needs rework to work on CI")]
    public async Task I_can_install_an_update_after_preparing_it()
    {
        // Arrange
        using var dummy = new DummyEnvironment(Path.Combine(TempDirPath, "Dummy"));

        var baseVersion = Version.Parse("1.0.0.0");

        var availableVersions = new[]
        {
            Version.Parse("1.0.0.0"),
            Version.Parse("2.0.0.0"),
            Version.Parse("3.0.0.0")
        };

        var expectedFinalVersion = Version.Parse("3.0.0.0");

        dummy.Setup(baseVersion, availableVersions);

        // Assert (current version)
        var oldVersion = Version.Parse(await dummy.RunDummyAsync("version"));
        oldVersion.Should().Be(baseVersion);

        // Act
        await dummy.RunDummyAsync("update");

        await Task.Delay(1000); // wait for updater to start
        await dummy.WaitUntilUpdaterExitsAsync();
        _testOutput.WriteLine(dummy.GetLastUpdaterLogs());

        // Assert (version after update)
        var newVersion = Version.Parse(await dummy.RunDummyAsync("version"));
        newVersion.Should().Be(expectedFinalVersion);
    }

    [Fact(Timeout = 10000, Skip = "Needs rework to work on CI")]
    public async Task I_can_install_an_update_after_preparing_it_and_have_the_application_restarted_automatically()
    {
        // Arrange
        using var dummy = new DummyEnvironment(Path.Combine(TempDirPath, "Dummy"));

        var baseVersion = Version.Parse("1.0.0.0");

        var availableVersions = new[]
        {
            Version.Parse("1.0.0.0"),
            Version.Parse("2.0.0.0"),
            Version.Parse("3.0.0.0")
        };

        var expectedFinalVersion = Version.Parse("3.0.0.0");

        dummy.Setup(baseVersion, availableVersions);

        // Act
        var args = new[] { "update-and-restart", "with", "extra", "arguments" };
        await dummy.RunDummyAsync(args);

        await Task.Delay(1000); // wait for updater to start
        await dummy.WaitUntilUpdaterExitsAsync();
        _testOutput.WriteLine(dummy.GetLastUpdaterLogs());

        // Wait until updatee has been ran a second time (we don't control this)
        while (!dummy.GetLastRunArguments(expectedFinalVersion).Any())
            await Task.Delay(100);

        // Assert
        dummy.GetLastRunArguments(expectedFinalVersion).Should().BeEquivalentTo(args);
    }
}
