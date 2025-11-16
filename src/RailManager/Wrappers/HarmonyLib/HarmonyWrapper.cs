using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;

namespace RailManager.Wrappers.HarmonyLib;

public delegate IHarmony HarmonyFactory(string harmonyId);

/// <summary>
///     IHarmony defines a minimal contract for interacting with Harmony patching functionality.
///     It mirrors key methods from the original Harmony class but allows for injection or mocking.
/// </summary>
[PublicAPI]
public interface IHarmony
{
    /// <inheritdoc cref="Harmony.PatchAll(Assembly)" />
    void PatchAll(Assembly assembly);

    /// <inheritdoc cref="Harmony.PatchCategory(Assembly, string)" />
    void PatchCategory(Assembly assembly, string category);

    /// <inheritdoc cref="Harmony.UnpatchCategory(Assembly, string)" />
    void UnpatchCategory(Assembly assembly, string category);

    /// <inheritdoc cref="Harmony.PatchAllUncategorized(Assembly)" />
    void PatchAllUncategorized(Assembly assembly);

    /// <inheritdoc cref="Harmony.UnpatchAll(string)" />
    void UnpatchAll(string id);
}

[ExcludeFromCodeCoverage]
public sealed class HarmonyWrapper(Harmony harmony) : IHarmony
{
    public static IHarmony CreateWrapper(string harmonyId) => new HarmonyWrapper(new(harmonyId));

    /// <inheritdoc />
    public void PatchAll(Assembly assembly) => harmony.PatchAll(assembly);

    /// <inheritdoc />
    public void PatchCategory(Assembly assembly, string category) => harmony.PatchCategory(assembly, category);

    /// <inheritdoc />
    public void UnpatchCategory(Assembly assembly, string category) => harmony.UnpatchCategory(assembly, category);

    /// <inheritdoc />
    public void PatchAllUncategorized(Assembly assembly) => harmony.PatchAllUncategorized(assembly);

    /// <inheritdoc />
    public void UnpatchAll(string id) => harmony.UnpatchAll(id);
}
