using JetBrains.Annotations;

namespace RailManager.Interfaces.Markers;

/// <summary>
///     Marker interface for plugins that want to use harmony to patch game code.
/// </summary>
[PublicAPI]
public interface IHarmonyPlugin : IPlugin;
