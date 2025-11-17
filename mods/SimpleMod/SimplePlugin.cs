using System;
using JetBrains.Annotations;
using RailManager.Interfaces;
using RailManager.Interfaces.Markers;
using Serilog;

namespace SimpleMod
{
    [UsedImplicitly]
    public class SimplePlugin : PluginBase, IHarmonyPlugin, ITopRightButtonPlugin
    {
        [CanBeNull]
        public static SimplePlugin Instance { get; private set; }

        [NotNull]
        public ILogger Logger { get; }

        public SimplePlugin([NotNull] IModdingContext moddingContext, [NotNull] IMod mod)
            : base(moddingContext, mod) {
            Instance = this;

            Logger = mod.CreateLogger();
            Logger.Information("Plugin ctor: {identifier}", mod.Definition.Identifier);
        }

        protected override void OnIsEnabledChanged() {
            
            base.OnIsEnabledChanged();
            Logger.Information("SimplePlugin: OnIsEnabledChanged: {isEnabled}", IsEnabled);
        }

        string ITopRightButtonPlugin.IconName => "Resources.Icon.png";

        string ITopRightButtonPlugin.Tooltip => "Tooltip";

        int ITopRightButtonPlugin.Index => 9;

        Action ITopRightButtonPlugin.OnClick => () => Logger.Information("SimplePlugin: TopRightButton Clicked");

        public void DoSomething() {
            Logger.Information("SimplePlugin: DoSomething Called");
        }
    }

    /// <summary>
    ///     When more than one top right button needed ...
    /// </summary>
    [UsedImplicitly]
    public class SecondTopRightButton : PluginBase, ITopRightButtonPlugin
    {
        public SecondTopRightButton([NotNull] IModdingContext moddingContext, [NotNull] IMod mod)
            : base(moddingContext, mod) {
        }

        string ITopRightButtonPlugin.IconName => "Resources.Icon2.png";

        string ITopRightButtonPlugin.Tooltip => "Tooltip2";

        int ITopRightButtonPlugin.Index => 9;

        Action ITopRightButtonPlugin.OnClick => () => SimplePlugin.Instance?.Logger.Information("SimplePlugin: TopRightButton2 Clicked");
    }
}
