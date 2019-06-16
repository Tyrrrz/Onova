using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Onova.Tests.Internal;

namespace Onova.Tests
{
    [TestFixture]
    public class DummyTests
    {
        private static string TempDirPath => Path.Combine(TestContext.CurrentContext.TestDirectory, "Temp");

        private static string StorageDirPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Onova", "Onova.Tests.Dummy");

        private static string UpdaterLogFilePath => Path.Combine(StorageDirPath, "Log.txt");

        [SetUp]
        public void Setup()
        {
            // Ensure temp directory exists and is empty
            DirectoryEx.Reset(TempDirPath);
        }

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

            // Delete temp directory
            if (Directory.Exists(TempDirPath))
                Directory.Delete(TempDirPath, true);

            // Delete storage directory
            if (Directory.Exists(StorageDirPath))
                Directory.Delete(StorageDirPath, true);
        }

        [Test]
        public async Task UpdateManager_CheckPerformUpdateAsync_Test()
        {
            using (var dummyEnvironment = new DummyEnvironment(TempDirPath))
            {
                // Arrange
                var baseVersion = Version.Parse("1.0.0.0");
                var availableVersions = new[]
                {
                    Version.Parse("1.0.0.0"),
                    Version.Parse("2.0.0.0"),
                    Version.Parse("3.0.0.0")
                };

                dummyEnvironment.Setup(baseVersion, availableVersions);

                // Assert current version
                var oldVersion = await dummyEnvironment.GetCurrentVersionAsync();
                Assert.That(oldVersion, Is.EqualTo(baseVersion), "Version before update");

                // Update dummy via Onova
                await dummyEnvironment.CheckPerformUpdateAsync();

                // Assert current version again
                var newVersion = await dummyEnvironment.GetCurrentVersionAsync();
                Assert.That(newVersion, Is.EqualTo(availableVersions.Max()), "Version after update");
            }
        }
    }
}