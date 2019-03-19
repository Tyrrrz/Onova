using System;
using System.IO;

namespace Onova.Internal
{
    internal partial class LockFile : IDisposable
    {
        private readonly FileStream _fileStream;

        public LockFile(FileStream fileStream)
        {
            _fileStream = fileStream;
        }

        public void Dispose() => _fileStream.Dispose();
    }

    internal partial class LockFile
    {
        public static LockFile TryAcquire(string filePath)
        {
            try
            {
                var fileStream = File.Open(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
                return new LockFile(fileStream);
            }
            catch
            {
                return null;
            }
        }
    }
}