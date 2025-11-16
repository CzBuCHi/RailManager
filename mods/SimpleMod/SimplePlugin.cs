using System;
using JetBrains.Annotations;
using RailManager.Interfaces;
using RailManager.Interfaces.Markers;
using Serilog;

namespace SimpleMod
{
    /// <summary>
    ///     Represents a minimal implementation of a plugin that demonstrates the required structure
    ///     and lifecycle integration for plugins in the modding framework.
    /// </summary>
    /// <remarks>
    ///     This class serves as a reference example for developers creating new plugins.
    ///     It inherits from <see cref="PluginBase{TPlugin}" /> and implements the essential constructor and lifecycle
    ///     callbacks.
    ///     Key requirements demonstrated:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Constructor must accept <see cref="IModdingContext" /> and <see cref="IMod" /> parameters.</description>
    ///         </item>
    ///         <item>
    ///             <description>Logger should be created via <c>mod.CreateLogger()</c> for proper identification.</description>
    ///         </item>
    ///         <item>
    ///             <description>Override <see cref="OnIsEnabledChanged" /> to react to enable/disable state changes.</description>
    ///         </item>
    ///         <item>
    ///             <description>Use <c>IsEnabled</c> property from base class to check current state.</description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Optional marker interface <see cref="IHarmonyPlugin" /> tells manager to apply all patches in
    ///                 this assembly.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Optional marker interface <see cref="ITopRightButtonPlugin" /> tells manager to add button to
    ///                 top right menu.
    ///             </description>
    ///         </item>
    ///     </list>
    ///     Plugins extending this pattern will be automatically discovered and managed by the modding system.
    /// </remarks>
    [UsedImplicitly]
    public class SimplePlugin : PluginBase<SimplePlugin>, IHarmonyPlugin, ITopRightButtonPlugin
    {
        [NotNull]
        public ILogger Logger { get; }

        public SimplePlugin([NotNull] IModdingContext moddingContext, [NotNull] IMod mod)
            : base(moddingContext, mod) {
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
    public class SecondTopRightButton : PluginBase<SecondTopRightButton>, ITopRightButtonPlugin
    {
        public SecondTopRightButton([NotNull] IModdingContext moddingContext, [NotNull] IMod mod)
            : base(moddingContext, mod) {
        }

        string ITopRightButtonPlugin.IconName => "Resources.Icon2.png";

        string ITopRightButtonPlugin.Tooltip => "Tooltip2";

        int ITopRightButtonPlugin.Index => 9;

        Action ITopRightButtonPlugin.OnClick => () => SimplePlugin.Instance.Logger.Information("SimplePlugin: TopRightButton2 Clicked");
    }
}
