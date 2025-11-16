using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using RailManager.Extensions;
using RailManager.HarmonyPatches;
using RailManager.Interfaces;
using RailManager.Interfaces.UI;
using RailManager.Wrappers.HarmonyLib;
using Serilog;

namespace RailManager;

/// <summary> Implementation of <see cref="IModdingContext" /> providing basic modding services. </summary>
[method: EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ModdingContext(IReadOnlyCollection<IMod> mods, ILogger logger, HarmonyFactory harmonyFactory)
    : IModdingContext
{
    [ExcludeFromCodeCoverage]
    public ModdingContext(IReadOnlyCollection<IMod> mods)
        : this(mods, Log.Logger.ForSourceContext(), HarmonyWrapper.CreateWrapper) {
    }

    /// <inheritdoc />
    public IReadOnlyCollection<IMod> Mods { get; } = mods;

    public ILogger Logger { get; } = logger;

    public HarmonyFactory HarmonyFactory { get; } = harmonyFactory;

    [ExcludeFromCodeCoverage] // wrapper
    public void RegisterWindow<TWindow>() where TWindow : ProgrammaticWindowBase => ProgrammaticWindowCreatorPatches.RegisterWindow<TWindow>();

    [ExcludeFromCodeCoverage] // wrapper
    public void OpenWindow<TWindow>() where TWindow : ProgrammaticWindowBase => ProgrammaticWindowCreatorPatches.GetWindow<TWindow>().ShowWindow();

    [ExcludeFromCodeCoverage] // wrapper
    public void CloseWindow<TWindow>() where TWindow : ProgrammaticWindowBase => ProgrammaticWindowCreatorPatches.GetWindow<TWindow>().CloseWindow();
}
