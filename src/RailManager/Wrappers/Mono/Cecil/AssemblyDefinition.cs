using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Mono.Cecil;
using _AssemblyDefinition = Mono.Cecil.AssemblyDefinition;
using _ReaderParameters = Mono.Cecil.ReaderParameters;

namespace RailManager.Wrappers.Mono.Cecil;

/// <summary>
///     Provides a mockable interface for static methods of <see cref="_AssemblyDefinition" />.
///     This enables unit testing scenarios where reading assemblies needs to be intercepted or stubbed.
/// </summary>
[PublicAPI]
public interface IAssemblyDefinitionStatic
{
    /// <inheritdoc cref="_AssemblyDefinition.ReadAssembly(string, _ReaderParameters)" />
    IAssemblyDefinition? ReadAssembly(string fileName, _ReaderParameters parameters);
}

/// <summary>
///     Defines a minimal, mockable contract for instance operations on <see cref="_AssemblyDefinition" />.
///     Used to abstract away direct dependency on Cecil types in business logic or mod loading code.
/// </summary>
[PublicAPI]
public interface IAssemblyDefinition : IDisposable
{
    /// <inheritdoc cref="_AssemblyDefinition.Write(string)" />
    void Write(string fileName);

    ModuleDefinition MainModule { get; }
}

[ExcludeFromCodeCoverage]
public sealed class AssemblyDefinitionStatic : IAssemblyDefinitionStatic
{
    public static readonly IAssemblyDefinitionStatic Instance = new AssemblyDefinitionStatic();
    
    /// <inheritdoc />
    public IAssemblyDefinition? ReadAssembly(string fileName, _ReaderParameters parameters) =>
        AssemblyDefinitionWrapper.CreateWrapper(_AssemblyDefinition.ReadAssembly(fileName, parameters));
}

[ExcludeFromCodeCoverage]
public sealed class AssemblyDefinitionWrapper(_AssemblyDefinition assemblyDefinition) : IAssemblyDefinition
{
    /// <summary>
    ///     Creates a new <see cref="IAssemblyDefinition" /> wrapper around an existing <see cref="_AssemblyDefinition" />
    ///     instance.
    /// </summary>
    /// <param name="assemblyDefinition">The existing Harmony instance to wrap.</param>
    /// <returns>An <see cref="IAssemblyDefinition" /> implementation that delegates to the provided instance.</returns>
    public static IAssemblyDefinition? CreateWrapper(_AssemblyDefinition? assemblyDefinition) =>
        assemblyDefinition != null ? new AssemblyDefinitionWrapper(assemblyDefinition) : null;

    /// <inheritdoc />
    public void Write(string fileName) => assemblyDefinition.Write(fileName);

    public ModuleDefinition MainModule => assemblyDefinition.MainModule;

    public void Dispose() {
        assemblyDefinition.Dispose();
    }
}
