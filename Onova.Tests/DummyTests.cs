using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Onova.Tests.Internal;

namespace Onova.Tests
{
    [TestFixture]
    public class DummyTests
    {
        private static string StorageDirPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Onova", "Onova.Tests.Dummy");

        private static string UpdaterLogFilePath => Path.Combine(StorageDirPath, "Log.txt");

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

            // Delete dummy environment
            DummyEnvironment.Delete();

            // Delete storage
            if (Directory.Exists(StorageDirPath))
                Directory.Delete(StorageDirPath, true);
        }

        [Test]
        public async Task UpdateManager_CheckPerformUpdateAsync_Test()
        {
            // Arrange
            DummyEnvironment.Setup(
                Version.Parse("1.0.0.0"),
                Version.Parse("1.0.0.0"), Version.Parse("2.0.0.0"), Version.Parse("3.0.0.0"));

            // Assert current version
            var oldVersion = await DummyEnvironment.GetCurrentVersionAsync();
            Assert.That(oldVersion, Is.EqualTo(Version.Parse("1.0.0.0")));

            // Update dummy via Onova
            await DummyEnvironment.CheckPerformUpdateAsync();

            // Assert current version again
            var newVersion = await DummyEnvironment.GetCurrentVersionAsync();
            Assert.That(newVersion, Is.EqualTo(Version.Parse("3.0.0.0")));
        }
    }
}