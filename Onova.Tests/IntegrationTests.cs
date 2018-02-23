using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Onova.Tests
{
    [TestFixture]
    public class IntegrationTests
    {
        [SetUp]
        public void Setup()
        {
            DummyHelper.SetupDummy();
        }

        [TearDown]
        public void Cleanup()
        {
            DummyHelper.DeleteDummy();
        }

        [Test]
        public async Task UpdateManager_PerformUpdateAsync_Test()
        {
            // Check current version
            var oldVersion = await DummyHelper.GetDummyVersionAsync();
            Assert.That(oldVersion, Is.EqualTo(Version.Parse("1.0.0.0")));

            // Update
            await DummyHelper.UpdateDummyAsync();

            // Check current version again
            var newVersion = await DummyHelper.GetDummyVersionAsync();
            Assert.That(newVersion, Is.EqualTo(Version.Parse("2.0.0.0")));
        }
    }
}