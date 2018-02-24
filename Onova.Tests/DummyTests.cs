using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Onova.Tests
{
    [TestFixture]
    public class DummyTests
    {
        private static string StorageDirPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Onova",
            "Onova.Tests.Dummy");

        [SetUp]
        public void Setup()
        {
            DummyHelper.SetupDummy();
        }

        [TearDown]
        public void Cleanup()
        {
            DummyHelper.DeleteDummy();

            if (Directory.Exists(StorageDirPath))
                Directory.Delete(StorageDirPath, true);
        }

        [Test]
        public async Task UpdateManager_PerformUpdateIfAvailableAsync_Test()
        {
            // Check current version
            var oldVersion = await DummyHelper.GetDummyVersionAsync();
            Assert.That(oldVersion, Is.EqualTo(Version.Parse("1.0.0.0")));

            // Update
            await DummyHelper.UpdateDummyAsync();

            // Check current version again
            var newVersion = await DummyHelper.GetDummyVersionAsync();
            Assert.That(newVersion, Is.EqualTo(Version.Parse("3.0.0.0")));
        }
    }
}