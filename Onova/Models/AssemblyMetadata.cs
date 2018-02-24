using System;
using System.IO;
using System.Reflection;
using JetBrains.Annotations;
using Onova.Internal;

namespace Onova.Models
{
    /// <summary>
    /// Contains information about an assembly.
    /// </summary>
    public class AssemblyMetadata
    {
        /// <summary>
        /// Assembly name.
        /// </summary>
        [NotNull]
        public string Name { get; }

        /// <summary>
        /// Assembly version.
        /// </summary>
        [NotNull]
        public Version Version { get; }

        /// <summary>
        /// Assembly file path.
        /// </summary>
        [NotNull]
        public string FilePath { get; }

        /// <summary>
        /// Assembly directory path.
        /// </summary>
        [NotNull]
        public string DirectoryPath { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="AssemblyMetadata"/>.
        /// </summary>
        public AssemblyMetadata(string name, Version version, string filePath, string directoryPath)
        {
            Name = name.GuardNotNull(nameof(name));
            Version = version.GuardNotNull(nameof(version));
            FilePath = filePath.GuardNotNull(nameof(filePath));
            DirectoryPath = directoryPath.GuardNotNull(nameof(directoryPath));
        }

        /// <summary>
        /// Initializes a new instance of <see cref="AssemblyMetadata"/>.
        /// </summary>
        public AssemblyMetadata(Assembly assembly)
        {
            assembly.GuardNotNull(nameof(assembly));

            Name = assembly.GetName().Name;
            Version = assembly.GetName().Version;
            FilePath = assembly.Location;
            DirectoryPath = Path.GetDirectoryName(FilePath);
        }
    }
}