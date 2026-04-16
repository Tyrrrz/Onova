using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using FluentAssertions;
using Onova.Services;
using PowerKit;
using PowerKit.Extensions;
using Xunit;

namespace Onova.Tests.Extracting;

public class NugetPackageSpecs : IDisposable
{
    private TempDirectory TempDir { get; } = TempDirectory.Create();

    public void Dispose() => TempDir.Dispose();

    private void CreateNugetPackage(
        string filePath,
        string rootDirPath,
        IReadOnlyDictionary<string, byte[]> entries
    )
    {
        using var zip = ZipFile.Open(filePath, ZipArchiveMode.Create);

        foreach (var (path, data) in entries)
            zip.CreateEntry($"{rootDirPath}/{path}").WriteAllBytes(data);
    }

    [Fact]
    public async Task ExtractPackageAsync_Test()
    {
        // Arrange
        var entries = new Dictionary<string, byte[]>
        {
            ["File1.bin"] = [1, 2, 3],
            ["File2.bin"] = [4, 5, 6],
            ["SubDir1/"] = [],
            ["SubDir1/File3.bin"] = [7, 8, 9],
            ["SubDir1/SubDir2/"] = [],
            ["SubDir1/SubDir2/File4.bin"] = [10, 11, 12],
        };

        var packageFilePath = Path.Combine(TempDir.Path, "Package.nupkg");
        CreateNugetPackage(packageFilePath, "Files", entries);

        var extractor = new NugetPackageExtractor("Files");

        var destDirPath = Path.Combine(TempDir.Path, "Output");

        // Act
        await extractor.ExtractPackageAsync(packageFilePath, destDirPath);

        // Assert
        foreach (var (path, expectedData) in entries)
        {
            var destEntryPath = Path.Combine(destDirPath, path);

            if (path.EndsWith("/"))
            {
                Directory.Exists(destEntryPath).Should().BeTrue();
            }
            else
            {
                var data = await File.ReadAllBytesAsync(destEntryPath);
                data.Should().BeEquivalentTo(expectedData);
            }
        }
    }
}
