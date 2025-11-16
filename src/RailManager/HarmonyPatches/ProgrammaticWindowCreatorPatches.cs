using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using RailManager.Interfaces.UI;
using UI;

namespace RailManager.HarmonyPatches;

[PublicAPI]
[HarmonyPatch]
public static class ProgrammaticWindowCreatorPatches
{
    private static Dictionary<Type, object> _RegisteredWindows = new();
    private static bool                     _Started;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ProgrammaticWindowCreator), "Start")]
    [ExcludeFromCodeCoverage]
    public static void Start(ProgrammaticWindowCreator __instance) {
        _Started = true;

        var methodInfo =
            typeof(ProgrammaticWindowCreator)
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .FirstOrDefault(o => o.IsGenericMethod && o.Name == "CreateWindow" && o.GetParameters().Length == 1);

        if (methodInfo == null) {
            throw new InvalidOperationException("Cannot find method UI.ProgrammaticWindowCreator:CreateWindow<TWindow>(Action<>).");
        }

        foreach (var pair in _RegisteredWindows) {
            methodInfo.MakeGenericMethod(pair.Key).Invoke(__instance, [pair.Value]);
        }
    }

    public static void RegisterWindow<TWindow>() where TWindow : ProgrammaticWindowBase {
        if (_Started) {
            throw new InvalidOperationException("Cannot register window: Game already started.");
        }

        var type = typeof(TWindow);
        _RegisteredWindows[type] = new Action<TWindow>(window => _RegisteredWindows[type] = window);
    }

    public static TWindow GetWindow<TWindow>() where TWindow : ProgrammaticWindowBase {
        if (!_Started) {
            throw new InvalidOperationException("Cannot get window: Game not started.");
        }

        var type = typeof(TWindow);
        _RegisteredWindows.TryGetValue(type, out var instance);
        return instance as TWindow ?? throw new InvalidOperationException($"Cannot find window {type}.");
    }
}
