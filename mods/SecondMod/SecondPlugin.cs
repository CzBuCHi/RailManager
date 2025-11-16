using JetBrains.Annotations;
using RailManager.Interfaces;
using SimpleMod;

namespace SecondMod
{
    [UsedImplicitly]
    public class SecondPlugin : PluginBase<SecondPlugin>
    {
        public SecondPlugin([NotNull] IModdingContext moddingContext, [NotNull] IMod mod)
            : base(moddingContext, mod) {
        }

        protected override void OnIsEnabledChanged() {
            base.OnIsEnabledChanged();
            // calls method from references mod
            SimplePlugin.Instance.DoSomething();
        }
    }
}
