using System.Collections.Generic;
using RailManager.Interfaces.UI;

namespace RailManager.Interfaces;

/// <summary>
///     An injectable interface that allows access to other mods and some quality-of-life functionality.
/// </summary>
public interface IModdingContext
{
    /// <summary>
    ///     Gets the list of all mods. This includes loaded, enabled, disabled, and failed mods.
    /// </summary>
    IReadOnlyCollection<IMod> Mods { get; }

    /// <summary>
    ///     Registers a window type with the application window manager.
    ///     The window must inherit from <see cref="ProgrammaticWindowBase" />.
    ///     Once registered, the window can be opened or closed using <see cref="OpenWindow{TWindow}" /> and
    ///     <see cref="CloseWindow{TWindow}" />.
    /// </summary>
    /// <typeparam name="TWindow">The type of the window to register, constrained to <see cref="ProgrammaticWindowBase" />.</typeparam>
    void RegisterWindow<TWindow>() where TWindow : ProgrammaticWindowBase;

    /// <summary>
    ///     Opens an instance of the specified programmatic window.
    ///     The window type must have been previously registered using <see cref="RegisterWindow{TWindow}" />.
    /// </summary>
    /// <typeparam name="TWindow">The type of the window to open, constrained to <see cref="ProgrammaticWindowBase" />.</typeparam>
    void OpenWindow<TWindow>() where TWindow : ProgrammaticWindowBase;

    /// <summary>
    ///     Closes the currently open instance of the specified window.
    ///     If no instance is open, this operation is a no-op.
    ///     The window type must derive from <see cref="ProgrammaticWindowBase" />.
    /// </summary>
    /// <typeparam name="TWindow">The type of the window to close, constrained to <see cref="ProgrammaticWindowBase" />.</typeparam>
    void CloseWindow<TWindow>() where TWindow : ProgrammaticWindowBase;
}
