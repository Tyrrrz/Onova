using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Onova.Models;

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

    internal string DirPath => Path.GetDirectoryName(FilePath)!;

    /// <summary>
    /// Initializes a new instance of <see cref="AssemblyMetadata" />.
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
    public static AssemblyMetadata FromAssembly(Assembly assembly, string assemblyFilePath) => new(
        assembly.GetName().Name!,
        assembly.GetName().Version!,
        assemblyFilePath
    );

    /// <summary>
    /// Extracts assembly metadata from given assembly.
    /// </summary>
    public static AssemblyMetadata FromAssembly(Assembly assembly)
    {
        if (string.IsNullOrEmpty(assembly.Location))
        {
            throw new InvalidOperationException(
                $"The location of assembly {assembly.GetName().FullName} could not be determined. " +
                "Use the `AssemblyMetadata.FromAssembly(Assembly assembly, string assemblyFilePath)` method to provide it explicitly."
            );
        }

        return FromAssembly(assembly, assembly.Location);
    }

    /// <summary>
    /// Extracts assembly metadata from entry assembly.
    /// </summary>
    public static AssemblyMetadata FromEntryAssembly()
    {
        // For most applications, the entry assembly is the entry point
        var assembly =
            Assembly.GetEntryAssembly() ??
            throw new InvalidOperationException("Can't get entry assembly.");

        if (!string.IsNullOrWhiteSpace(assembly.Location))
            return FromAssembly(assembly, assembly.Location);

        // For self-contained applications, the executable is the entry point
        var filePath =
            Process.GetCurrentProcess().MainModule?.FileName ??
            throw new InvalidOperationException("Can't get current process main module.");

        return FromAssembly(assembly, filePath);
    }
}