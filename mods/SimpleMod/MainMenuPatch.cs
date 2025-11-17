using HarmonyLib;
using JetBrains.Annotations;
using RailManager.Interfaces.Markers;
using UI.Menu;

namespace SimpleMod
{
    /// <summary>
    /// Dummy patch to write something to log to show <see cref="IHarmonyPlugin"/> marker working.
    /// </summary>
    [HarmonyPatch]
    [UsedImplicitly]
    public static class MainMenuPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MainMenu), "ShouldShowEditor")]
        public static bool ShouldShowEditorPrefix() {
            SimplePlugin.Instance?.Logger.Information("--- MainMenu::ShouldShowEditor::Prefix patch from dummy called: {IsEnabled}", SimplePlugin.Instance.IsEnabled);
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MainMenu), "ShouldShowEditor")]
        public static void ShouldShowEditorPostfix() {
            SimplePlugin.Instance?.Logger.Information("--- MainMenu::ShouldShowEditor::Postfix patch from dummy called: {IsEnabled}", SimplePlugin.Instance.IsEnabled);
        }
    }
}
