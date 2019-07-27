using System;
using System.IO;
using System.Reflection;
using System.Runtime.Versioning;
using JetBrains.Annotations;
using Onova.Internal;

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
        /// Initializes a new instance of <see cref="AssemblyMetadata"/>.
        /// </summary>
        public AssemblyMetadata(string name, Version version, string filePath)
        {
            Name = name.GuardNotNull(nameof(name));
            Version = version.GuardNotNull(nameof(version));
            FilePath = filePath.GuardNotNull(nameof(filePath));
        }
    }

    public partial class AssemblyMetadata
    {
        /// <summary>
        /// Extracts assembly metadata from given assembly.
        /// </summary>
        public static AssemblyMetadata FromAssembly(Assembly assembly)
        {
            assembly.GuardNotNull(nameof(assembly));

            var name = assembly.GetName().Name;
            var version = assembly.GetName().Version;
            var filePath = assembly.Location;

            return new AssemblyMetadata(name, version, filePath);
        }

        /// <summary>
        /// Extracts assembly metadata from entry assembly.
        /// </summary>
        public static AssemblyMetadata FromEntryAssembly()
        {
            var entryAssembly = Assembly.GetEntryAssembly();
            string framework = entryAssembly.GetCustomAttribute<TargetFrameworkAttribute>().FrameworkName;
            if (framework.StartsWith(".NETCoreApp"))
            {
                var name        = entryAssembly.GetName().Name;
                var version     = entryAssembly.GetName().Version;
                var filePath    = entryAssembly.Location;

                if (filePath.EndsWith(".dll"))
                {
                    filePath = Path.ChangeExtension(filePath, ".exe");
                    if (!File.Exists(filePath))
                        throw new FileNotFoundException($"Currently executing assembly was DLL, so assumed WPF/WinForms .NET Core, but exe with same name {Path.GetFileName(filePath)} does not exist.");
                }

                return new AssemblyMetadata(name, version, filePath);
            }

            return FromAssembly(entryAssembly);
        }
    }
}