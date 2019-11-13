using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Onova.Tests.Internal;

namespace Onova.Tests
{
    [TestFixture]
    public class DummyTests
    {
        private static string TempDirPath => Path.Combine(TestContext.CurrentContext.TestDirectory, "Temp");

        private static string StorageDirPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Onova", "Onova.Tests.Dummy");

        private static string UpdaterLogFilePath => Path.Combine(StorageDirPath, "Log.txt");

        [SetUp]
        public void Setup()
        {
            // Ensure temp directory exists and is empty
            DirectoryEx.Reset(TempDirPath);
        }

        [TearDown]
        public void Cleanup()
        {
            // Wait for files to be released
            Thread.Sleep(50);

            // Attach updater log
            if (File.Exists(UpdaterLogFilePath))
            {
                var log = File.ReadAllText(UpdaterLogFilePath);
                TestContext.Out.WriteLine($"Updater log:{Environment.NewLine}{log}");
            }

            // Delete temp directory
            if (Directory.Exists(TempDirPath))
                Directory.Delete(TempDirPath, true);

            // Delete storage directory
            if (Directory.Exists(StorageDirPath))
                Directory.Delete(StorageDirPath, true);
        }

        [Test]
        public async Task UpdateManager_CheckPerformUpdateAsync_Test()
        {
            using var dummy = new DummyEnvironment(TempDirPath);

            // Arrange
            var baseVersion = Version.Parse("1.0.0.0");
            var availableVersions = new[]
            {
                Version.Parse("1.0.0.0"),
                Version.Parse("2.0.0.0"),
                Version.Parse("3.0.0.0")
            };

            dummy.Setup(baseVersion, availableVersions);

            // Assert current version
            var oldVersion = await dummy.RunDummyAsync("version");
            Assert.That(Version.Parse(oldVersion), Is.EqualTo(baseVersion), "Version before update");

            // Act
            await dummy.RunDummyAsync("update");

            // Assert current version again
            var newVersion = await dummy.RunDummyAsync("version");
            Assert.That(Version.Parse(newVersion), Is.EqualTo(availableVersions.Max()), "Version after update");
        }

        [Test]
        public async Task UpdateManager_CheckPerformUpdateAsync_Restart_Test()
        {
            using var dummy = new DummyEnvironment(TempDirPath);

            // Arrange
            var baseVersion = Version.Parse("1.0.0.0");
            var availableVersions = new[]
            {
                Version.Parse("1.0.0.0"),
                Version.Parse("2.0.0.0"),
                Version.Parse("3.0.0.0")
            };

            dummy.Setup(baseVersion, availableVersions);

            // Act
            const string args = "update-and-restart and some extra arguments";
            await dummy.RunDummyAsync(args);

            // Wait a bit for update and restart
            await Task.Delay(50);

            // Assert
            Assert.That(dummy.GetLastRunArguments(availableVersions.Max()), Is.EqualTo(args), "Command line arguments from last run");
        }
    }
}