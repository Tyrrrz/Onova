using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using Onova.Services;
using Onova.Tests.Internal;

namespace Onova.Tests
{
    [TestFixture]
    public class WebPackageResolverTests
    {
        private static string TempDirPath => Path.Combine(TestContext.CurrentContext.TestDirectory, "Temp");

        private static WebPackageResolver CreateWebPackageResolver() =>
            new WebPackageResolver("https://raw.githubusercontent.com/Tyrrrz/OnovaTestRepo/master/TestWebPackageManifest.txt");

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
            var resolver = CreateWebPackageResolver();

            // Act
            var versions = await resolver.GetPackageVersionsAsync();

            // Assert
            Assert.That(versions, Is.EquivalentTo(new[]
            {
                Version.Parse("1.0"),
                Version.Parse("2.0"),
                Version.Parse("3.0")
            }));
        }

        [Test]
        public async Task DownloadPackageAsync_Test()
        {
            // Arrange
            var destFilePath = Path.Combine(TempDirPath, "Output.onv");
            var resolver = CreateWebPackageResolver();

            // Act
            await resolver.DownloadPackageAsync(Version.Parse("2.0"), destFilePath);

            // Assert
            Assert.That(File.Exists(destFilePath), "File exists");
            Assert.That(File.ReadAllText(destFilePath), Is.EqualTo("Hello world"), "File content");
        }
    }
}