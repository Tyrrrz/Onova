using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using NUnit.Framework;
using Onova.Services;
using Onova.Tests.Internal;

namespace Onova.Tests
{
    [TestFixture]
    public class NugetPackageResolverTests
    {
        private static string TempDirPath => Path.Combine(TestContext.CurrentContext.TestDirectory, "Temp");

        private static NugetPackageResolver CreateNugetPackageResolver() =>
            new NugetPackageResolver("https://www.myget.org/F/tyrrrz-test/api/v3/index.json", "OnovaTest");

        [SetUp]
        public void Setup()
        {
            // Ensure temp directory exists and is empty
            DirectoryEx.Reset(TempDirPath);
        }


        [TearDown]
        public void Cleanup()
        {
            // Delete temp directory
            if (Directory.Exists(TempDirPath))
                Directory.Delete(TempDirPath, true);
        }

        [Test]
        public async Task GetPackageVersionsAsync_Test()
        {
            // Arrange
            var resolver = CreateNugetPackageResolver();

            // Act
            var versions = await resolver.GetPackageVersionsAsync();

            // Assert
            Assert.That(versions, Is.EquivalentTo(new[]
            {
                Version.Parse("1.0.0"),
                Version.Parse("2.0.0"),
                Version.Parse("3.0.0")
            }));
        }

        [Test]
        public async Task DownloadPackageAsync_Test()
        {
            // Arrange
            var destFilePath = Path.Combine(TempDirPath, "Output.onv");
            var resolver = CreateNugetPackageResolver();

            // Act
            await resolver.DownloadPackageAsync(Version.Parse("2.0.0"), destFilePath);

            // Assert
            Assert.That(File.Exists(destFilePath), "File exists");

            using var zip = ZipFile.OpenRead(destFilePath);

            var content = zip.GetEntry("Files/Content.txt").ReadAllText();
            Assert.That(content, Is.EqualTo("Hello world"), "File content");
        }
    }
}