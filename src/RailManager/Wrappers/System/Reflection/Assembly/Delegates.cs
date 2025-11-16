using _Assembly = System.Reflection.Assembly;

namespace RailManager.Wrappers.System.Reflection.Assembly;

/// <summary>
///     Delegate that abstracts the static <see cref="_Assembly.LoadFrom(string)" /> method,
///     enabling dependency injection and full mocking of assembly loading in unit tests.
/// </summary>
/// <param name="assemblyFile">The path to the assembly file to load.</param>
/// <returns>
///     The loaded <see cref="_Assembly" /> instance, or <c>null</c> if loading fails
///     (consistent with <c>Assembly.LoadFrom</c> behavior when exceptions are caught or suppressed).
/// </returns>
/// <inheritdoc cref="_Assembly.LoadFrom(string)" />
public delegate _Assembly? LoadFrom(string assemblyFile);
