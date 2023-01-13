using System;
using System.Diagnostics;
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
            throw new InvalidOperationException($"The location of assembly {assembly.GetName().FullName} could not be determined. " +
                                                "Use AssemblyMetadata.FromAssembly(Assembly assembly, string assemblyFilePath) method instead");
        }
        return FromAssembly(assembly, assembly.Location);
    }

    /// <summary>
    /// Extracts assembly metadata from entry assembly.
    /// </summary>
    public static AssemblyMetadata FromEntryAssembly()
    {
        var assembly = Assembly.GetEntryAssembly() ?? throw new InvalidOperationException("Can't get entry assembly.");
        if (string.IsNullOrEmpty(assembly.Location))
        {
            // The assembly was published as a [single executable](https://learn.microsoft.com/en-us/dotnet/core/deploying/single-file/overview)
            // Location returning an empty string is [documented](https://learn.microsoft.com/en-us/dotnet/api/System.Reflection.Assembly.Location#remarks)
            // > In .NET 5 and later versions, for bundled assemblies, the value returned is an empty string.
            return FromAssembly(assembly, Process.GetCurrentProcess().MainModule?.FileName ?? throw new InvalidOperationException("Can't get current process main module."));
        }
        return FromAssembly(assembly, assembly.Location);
    }
}