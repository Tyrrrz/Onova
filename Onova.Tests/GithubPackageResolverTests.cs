using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using Onova.Services;
using Onova.Tests.Internal;

namespace Onova.Tests
{
    [TestFixture]
    public class GithubPackageResolverTests
    {
        private static string TempDirPath => Path.Combine(TestContext.CurrentContext.TestDirectory, "Temp");

        private static GithubPackageResolver CreateGithubPackageResolver()
            => new GithubPackageResolver("Tyrrrz", "OnovaTestRepo", "*.onv");

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
            var resolver = CreateGithubPackageResolver();

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
            var resolver = CreateGithubPackageResolver();

            // Act
            await resolver.DownloadPackageAsync(Version.Parse("2.0"), destFilePath);

            // Assert
            Assert.That(File.Exists(destFilePath), "File exists");
            Assert.That(File.ReadAllText(destFilePath), Is.EqualTo("Hello world"), "File content");
        }
    }
}