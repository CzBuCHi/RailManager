using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using UI;
using UI.Builder;
using UI.Common;
using UnityEngine;

namespace RailManager.Interfaces.UI;

/// <summary>
///     Base class for programmatically created mod UI windows in the Railroader mod manager.
///     Inheriting classes define custom UI content via <see cref="Populate" /> and are managed
///     automatically by the mod UI system when registered and opened through the manager.
/// </summary>
/// <remarks>
///     <para>
///         This class integrates with the game's native <c>Window</c> system and <see cref="UIPanel" /> builder
///         to provide a consistent, disposable, and lifecycle-aware UI window experience.
///     </para>
///     <para>
///         Key lifecycle events:
///         <list type="bullet">
///             <item>
///                 <description>
///                     <c>Awake</c> — Binds to the attached <c>Window</c> component and subscribes to visibility
///                     changes.
///                 </description>
///             </item>
///             <item>
///                 <description><c>OnWindowOpen</c> — Called when the window becomes visible; override for initialization.</description>
///             </item>
///             <item>
///                 <description><c>OnWindowClosed</c> — Called when the window is hidden; override for cleanup.</description>
///             </item>
///             <item>
///                 <description><c>OnDestroy</c> — Unsubscribes from events to prevent leaks.</description>
///             </item>
///             <item>
///                 <description><c>OnDisable</c> — Disposes the current panel to free UI resources.</description>
///             </item>
///         </list>
///     </para>
///     <para>
///         To create a custom window:
///         <list type="number">
///             <item>Derive from <c>ProgrammaticWindowBase</c>.</item>
///             <item>Implement <see cref="Populate" /> to build UI using <see cref="UIPanelBuilder" />.</item>
///             <item>Override <see cref="Sizing" /> to define min/max size constraints.</item>
///             <item>Optionally override <see cref="DefaultPosition" /> and <see cref="DefaultSize" />.</item>
///         </list>
///     </para>
/// </remarks>
/// <seealso cref="IProgrammaticWindow" />
[PublicAPI]
[ExcludeFromCodeCoverage] // Unity
public abstract class ProgrammaticWindowBase : MonoBehaviour, IProgrammaticWindow
{
    /// <summary>
    ///     Gets the UI builder assets used to construct the window content.
    ///     Automatically assigned by the game engine before the window is shown.
    /// </summary>
    /// <value>A non-null <see cref="UIBuilderAssets" /> instance provided by the game.</value>
    public UIBuilderAssets BuilderAssets { get; set; } = null!;

    /// <summary>
    ///     Gets the unique identifier for this window type, based on the full class name.
    ///     Used by the mod manager for registration and instance tracking.
    /// </summary>
    public string WindowIdentifier => GetType().FullName!;

    /// <summary>
    ///     Gets the default size of the window when first shown.
    ///     Defaults to the minimum size defined in <see cref="Sizing" />.
    /// </summary>
    public Vector2Int DefaultSize => Sizing.MinSize;

    /// <summary>
    ///     Gets the default screen position for the window.
    ///     Override to change initial placement (e.g., top-left, center, etc.).
    /// </summary>
    public virtual Window.Position DefaultPosition => Window.Position.Center;

    /// <summary>
    ///     Gets the sizing constraints (minimum and maximum dimensions) for this window.
    ///     Must be implemented by derived classes.
    /// </summary>
    public abstract Window.Sizing Sizing { get; }

    /// <summary>
    ///     Gets the underlying game <c>Window</c> component attached to this GameObject.
    ///     Automatically assigned in <c>Awake</c>.
    /// </summary>
    protected Window Window { get; private set; } = null!;

    private UIPanel? _Panel;

    /// <summary>
    ///     Called by Unity when the object becomes enabled and active.
    ///     Initializes the reference to the <c>Window</c> component and subscribes to visibility events.
    /// </summary>
    public virtual void Awake() {
        Window = GetComponent<Window>()!;
        Window.OnShownDidChange += WindowOnOnShownDidChange;
    }

    /// <summary>
    ///     Called by Unity when the behaviour is destroyed.
    ///     Unsubscribes from window events to prevent memory leaks.
    /// </summary>
    public virtual void OnDestroy() {
        Window.OnShownDidChange -= WindowOnOnShownDidChange;
    }

    /// <summary>
    ///     Called by Unity when the behaviour becomes disabled.
    ///     Disposes the current UI panel to release native and managed resources.
    /// </summary>
    public void OnDisable() {
        _Panel?.Dispose();
        _Panel = null;
    }

    /// <summary>
    ///     Internal handler for window visibility changes.
    ///     Dispatches to <see cref="OnWindowOpen" /> or <see cref="OnWindowClosed" /> accordingly.
    /// </summary>
    /// <param name="isShown"><c>true</c> if the window is now visible; <c>false</c> otherwise.</param>
    private void WindowOnOnShownDidChange(bool isShown) {
        if (isShown) {
            OnWindowOpen();
        } else {
            OnWindowClosed();
        }
    }

    /// <summary>
    ///     Called when the window is shown.
    ///     Override to perform initialization that requires the window to be visible
    ///     (e.g., focusing controls, loading dynamic data).
    /// </summary>
    protected virtual void OnWindowOpen() {
    }

    /// <summary>
    ///     Called when the window is hidden.
    ///     Override to clean up resources, save state, or reset UI.
    /// </summary>
    protected virtual void OnWindowClosed() {
    }

    /// <summary>
    ///     Shows the window and builds its UI content using the provided <see cref="BuilderAssets" />.
    ///     If the window is already shown, the existing panel is disposed and rebuilt.
    /// </summary>
    /// <remarks>
    ///     This method is typically called by the mod UI manager, not directly.
    /// </remarks>
    public void ShowWindow() {
        _Panel?.Dispose();
        _Panel = UIPanel.Create(Window.contentRectTransform!, BuilderAssets, Populate);
        Window.ShowWindow();
    }

    /// <summary>
    ///     Closes the window if it is currently shown.
    ///     Triggers <see cref="OnWindowClosed" /> and disposes the UI panel.
    /// </summary>
    public void CloseWindow() {
        if (Window.IsShown) {
            Window.CloseWindow();
        }
    }

    /// <summary>
    ///     Populates the window content using the provided UI builder.
    ///     Must be implemented by derived classes to define the actual UI layout.
    /// </summary>
    /// <param name="builder">The <see cref="UIPanelBuilder" /> used to construct UI elements.</param>
    protected abstract void Populate(UIPanelBuilder builder);
}
