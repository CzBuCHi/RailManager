using System;
using JetBrains.Annotations;

namespace RailManager.Interfaces;

/// <summary>
///     Base class for all plugins, providing common functionality and modding context access.
/// </summary>
/// <remarks>
///     This class is intended to be inherited by concrete plugin implementations.
/// </remarks>
[PublicAPI]
public abstract class PluginBase : IPlugin
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PluginBase" /> class.
    /// </summary>
    /// <param name="moddingContext">The modding context.</param>
    /// <param name="mod">The mod definition.</param>
    /// <exception cref="InvalidOperationException"> Thrown if an instance of this type already exists. </exception>
    protected PluginBase(IModdingContext moddingContext, IMod mod) {
        ModdingContext = moddingContext;
        Mod = mod;
    }

    /// <inheritdoc />
    public IModdingContext ModdingContext { get; }

    /// <inheritdoc />
    public IMod Mod { get; }

    private bool _IsEnabled;

    /// <inheritdoc />
    public bool IsEnabled {
        get => _IsEnabled;
        set {
            if (_IsEnabled == value) {
                return;
            }

            _IsEnabled = value;
            OnIsEnabledChanged();
        }
    }

    /// <summary>
    ///     Called when the <see cref="IsEnabled" /> property changes.
    /// </summary>
    /// <remarks>
    ///     Override this method to handle enable/disable events.
    ///     The base implementation does nothing.
    /// </remarks>
    protected virtual void OnIsEnabledChanged() {
    }
}
