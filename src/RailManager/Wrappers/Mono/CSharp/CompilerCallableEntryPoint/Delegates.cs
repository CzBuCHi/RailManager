// resharper disable all

using _TextWriter = System.IO.TextWriter;
using _CompilerCallableEntryPoint = Mono.CSharp.CompilerCallableEntryPoint;

namespace RailManager.Wrappers.Mono.CSharp.CompilerCallableEntryPoint;

/// <summary>
///     Delegate that wraps the static <c>CompilerCallableEntryPoint.InvokeCompiler</c> method,
///     enabling dependency injection and mocking in unit tests where direct static calls would
///     otherwise prevent testability.
/// </summary>
/// <param name="args">The command-line arguments to pass to the C# compiler.</param>
/// <param name="error">A <see cref="_TextWriter" /> to capture error output from the compiler.</param>
/// <returns>
///     <c>true</c> if compilation succeeded; <c>false</c> if there were errors or warnings
///     (depending on compiler configuration).
/// </returns>
/// <remarks>
///     This delegate exists solely to abstract away the static method call on
///     <see cref="_CompilerCallableEntryPoint" />. It allows higher-level code
///     (e.g., mod script compilers) to inject a mock or alternative implementation during testing.
/// </remarks>
/// <inheritdoc cref="_CompilerCallableEntryPoint.InvokeCompiler(string[], _TextWriter)" />
public delegate bool InvokeCompiler(string[] args, _TextWriter error);
