using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
using Onova.Services;

namespace Onova.Tests
{
    [TestFixture]
    public class IntegrationTests
    {
        private static string TestDirPath => TestContext.CurrentContext.TestDirectory;
        private static string TempDirPath => Path.Combine(TestDirPath, "Temp");

        private static string StorageDirPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Onova",
            Assembly.GetExecutingAssembly().GetName().Name);

        [TearDown]
        public void Cleanup()
        {
            if (Directory.Exists(StorageDirPath))
                Directory.Delete(StorageDirPath, true);
            if (Directory.Exists(TempDirPath))
                Directory.Delete(TempDirPath, true);
        }

        [Test]
        public async Task GithubPackageResolver_GetAllVersionsAsync_Test()
        {
            var expectedVersions = new[] {Version.Parse("1.0"), Version.Parse("2.0")};

            var resolver = new GithubPackageResolver("Tyrrrz", "OnovaTestRepo", "Test.onv");
            var versions = await resolver.GetAllVersionsAsync();

            Assert.That(versions, Is.Not.Null);
            Assert.That(versions, Is.EquivalentTo(expectedVersions));
        }

        [Test]
        public async Task GithubPackageResolver_GetPackageAsync_Test()
        {
            const string expectedContent = "Hello world";

            var resolver = new GithubPackageResolver("Tyrrrz", "OnovaTestRepo", "Test.onv");
            var stream = await resolver.GetPackageAsync(Version.Parse("2.0"));

            Assert.That(stream, Is.Not.Null);

            using (stream)
            using (var reader = new StreamReader(stream))
            {
                var content = await reader.ReadToEndAsync();
                Assert.That(content, Is.EqualTo(expectedContent));
            }
        }
    }
}