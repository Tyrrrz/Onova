using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;

namespace Onova.Models;

/// <summary>
/// Contains information about an assembly.
/// </summary>
public partial class AssemblyMetadata(string name, Version version, string filePath)
{
    /// <summary>
    /// Assembly name.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Assembly version.
    /// </summary>
    public Version Version { get; } = version;

    /// <summary>
    /// Assembly file path.
    /// </summary>
    public string FilePath { get; } = Path.GetFullPath(filePath);

    internal string DirPath { get; } = Path.GetDirectoryName(filePath)!;
}

public partial class AssemblyMetadata
{
    /// <summary>
    /// Extracts assembly metadata from the specified assembly.
    /// The specified path is used to override the executable file path in case the assembly is not available on disk.
    /// </summary>
    public static AssemblyMetadata FromAssembly(Assembly assembly, string assemblyFilePath) =>
        new(
            assembly.GetName().Name
                ?? throw new InvalidOperationException("Provided assembly's name is <null>."),
            assembly.GetName().Version
                ?? throw new InvalidOperationException("Provided assembly's version is <null>."),
            assemblyFilePath
        );

    /// <summary>
    /// Extracts assembly metadata from the specified assembly.
    /// </summary>
    [RequiresAssemblyFiles(
        "This method requires the specified assembly's file path to be available."
    )]
    public static AssemblyMetadata FromAssembly(Assembly assembly)
    {
        if (string.IsNullOrEmpty(assembly.Location))
        {
            throw new InvalidOperationException(
                $"The location of assembly {assembly.GetName().FullName} could not be determined. "
                    + "Use the `AssemblyMetadata.FromAssembly(Assembly assembly, string assemblyFilePath)` method to provide it explicitly."
            );
        }

        return FromAssembly(assembly, assembly.Location);
    }

    /// <summary>
    /// Extracts assembly metadata from the entry assembly.
    /// </summary>
    [UnconditionalSuppressMessage(
        "SingleFile",
        "IL3000:Avoid accessing Assembly file path when publishing as a single file",
        Justification = "The return value of the method is checked to ensure the assembly location is available."
    )]
    public static AssemblyMetadata FromEntryAssembly()
    {
        // For most applications, the entry assembly is the entry point
        var assembly =
            Assembly.GetEntryAssembly()
            ?? throw new InvalidOperationException("Failed to get the entry assembly.");

        if (!string.IsNullOrWhiteSpace(assembly.Location))
            return FromAssembly(assembly, assembly.Location);

        // For single-file applications, the executable is the entry point
        var filePath =
            Environment.ProcessPath
            ?? throw new InvalidOperationException(
                "Failed to get the current process's entry point."
            );

        return FromAssembly(assembly, filePath);
    }
}
