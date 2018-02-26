using System;
using System.IO;
using System.Threading;
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

        [TearDown]
        public void Cleanup()
        {
            Thread.Sleep(50); // wait for files to be released

            DummyHelper.DeleteDummy();

            if (Directory.Exists(StorageDirPath))
                Directory.Delete(StorageDirPath, true);
        }

        [Test]
        public async Task UpdateManager_PerformUpdateIfAvailableAsync_HigherVersionAvailable_Test()
        {
            // Arrange
            DummyHelper.SetupDummy(
                Version.Parse("1.0.0.0"),
                Version.Parse("1.0.0.0"), Version.Parse("2.0.0.0"), Version.Parse("3.0.0.0"));

            // Assert current version
            var oldVersion = await DummyHelper.GetDummyVersionAsync();
            Assert.That(oldVersion, Is.EqualTo(Version.Parse("1.0.0.0")));

            // Update dummy via Onova
            await DummyHelper.UpdateDummyAsync();

            // Assert current version again
            var newVersion = await DummyHelper.GetDummyVersionAsync();
            Assert.That(newVersion, Is.EqualTo(Version.Parse("3.0.0.0")));
        }

        [Test]
        public async Task UpdateManager_PerformUpdateIfAvailableAsync_AlreadyHighestVersion_Test()
        {
            // Arrange
            DummyHelper.SetupDummy(
                Version.Parse("3.0.0.0"),
                Version.Parse("1.0.0.0"), Version.Parse("2.0.0.0"), Version.Parse("3.0.0.0"));

            // Assert current version
            var oldVersion = await DummyHelper.GetDummyVersionAsync();
            Assert.That(oldVersion, Is.EqualTo(Version.Parse("3.0.0.0")));

            // Update dummy via Onova
            await DummyHelper.UpdateDummyAsync();

            // Assert current version again
            var newVersion = await DummyHelper.GetDummyVersionAsync();
            Assert.That(newVersion, Is.EqualTo(Version.Parse("3.0.0.0")));
        }
    }
}