using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using Onova.Services;

namespace Onova.Tests
{
    [TestFixture]
    public class IntegrationTests
    {
        [Test]
        public async Task GithubPackageResolver_GetAllVersionsAsync_Test()
        {
            var resolver = new GithubPackageResolver("Tyrrrz", "OnovaTestRepo", "Test.onv");

            var expectedVersions = new[] {Version.Parse("1.0"), Version.Parse("2.0")};
            var versions = await resolver.GetAllVersionsAsync();

            Assert.That(versions, Is.Not.Null);
            Assert.That(versions, Is.EquivalentTo(expectedVersions));
        }

        [Test]
        public async Task GithubPackageResolver_GetPackageAsync_Test()
        {
            var resolver = new GithubPackageResolver("Tyrrrz", "OnovaTestRepo", "Test.onv");

            var stream = await resolver.GetPackageAsync(Version.Parse("2.0"));

            Assert.That(stream, Is.Not.Null);

            using (stream)
            using (var reader = new StreamReader(stream))
            {
                var content = await reader.ReadToEndAsync();
                Assert.That(content, Is.EqualTo("Hello world"));
            }
        }
    }
}