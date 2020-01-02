using System;
using System.Reflection;

namespace Onova.Models
{
    /// <summary>
    /// Contains information about an assembly.
    /// </summary>
    public partial class AssemblyMetadata
    {
        /// <summary>
        /// Assembly name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Assembly version.
        /// </summary>
        public Version Version { get; }

        /// <summary>
        /// Assembly file path.
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="AssemblyMetadata"/>.
        /// </summary>
        public AssemblyMetadata(string name, Version version, string filePath)
        {
            Name = name;
            Version = version;
            FilePath = filePath;
        }
    }

    public partial class AssemblyMetadata
    {
        /// <summary>
        /// Extracts assembly metadata from given assembly.
        /// The specified path is used to override the executable file path in case the assembly is not meant to run directly.
        /// </summary>
        public static AssemblyMetadata FromAssembly(Assembly assembly, string assemblyFilePath)
        {
            var name = assembly.GetName().Name;
            var version = assembly.GetName().Version;
            var filePath = assemblyFilePath;

            return new AssemblyMetadata(name, version, filePath);
        }

        /// <summary>
        /// Extracts assembly metadata from given assembly.
        /// </summary>
        public static AssemblyMetadata FromAssembly(Assembly assembly) => FromAssembly(assembly, assembly.Location);

        /// <summary>
        /// Extracts assembly metadata from entry assembly.
        /// </summary>
        public static AssemblyMetadata FromEntryAssembly() => FromAssembly(Assembly.GetEntryAssembly());
    }
}