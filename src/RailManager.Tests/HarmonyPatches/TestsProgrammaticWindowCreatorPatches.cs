using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using RailManager.HarmonyPatches;
using RailManager.Interfaces.UI;
using Shouldly;
using UI.Builder;
using UI.Common;

namespace RailManager.Tests.HarmonyPatches;

[Collection("ProgrammaticWindowCreatorPatches")]
public class TestsProgrammaticWindowCreatorPatches
{
    private readonly FieldInfo                _StartedField = typeof(ProgrammaticWindowCreatorPatches).GetField("_Started", BindingFlags.Static | BindingFlags.NonPublic)!;
    private readonly Dictionary<Type, object> _RegisteredWindows;

    public TestsProgrammaticWindowCreatorPatches() {
        _RegisteredWindows = (Dictionary<Type, object>)typeof(ProgrammaticWindowCreatorPatches).GetField("_RegisteredWindows", BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null!)!;
        _RegisteredWindows.Clear();
        _StartedField.SetValue(null!, false);
    }

    [Fact]
    public void RegisterWindow_ThrowsAfterStart() {
        // Arrange
        _StartedField.SetValue(null!, true);

        // Act
        var act = ProgrammaticWindowCreatorPatches.RegisterWindow<TestWindow>;

        // Assert
        act.ShouldThrow<InvalidOperationException>().Message.ShouldBe("Cannot register window: Game already started.");
    }

    [Fact]
    public void AddWindowToDictionary() {
        // Act
        ProgrammaticWindowCreatorPatches.RegisterWindow<TestWindow>();

        // Assert
        _RegisteredWindows.ShouldContainKey(typeof(TestWindow));
        _RegisteredWindows[typeof(TestWindow)].ShouldBeOfType<Action<TestWindow>>();
    }

    [Fact]
    public void GetWindow_ThrowsBeforeStart() {
        // Arrange
        ProgrammaticWindowCreatorPatches.RegisterWindow<TestWindow>();

        // Act
        var act = ProgrammaticWindowCreatorPatches.GetWindow<TestWindow>;

        // Assert
        act.ShouldThrow<InvalidOperationException>().Message.ShouldBe("Cannot get window: Game not started.");
    }

    [Fact]
    public void GetWindow_ThrowsWhenUnregistered() {
        // Arrange
        _StartedField.SetValue(null!, true);

        // Act
        var act = ProgrammaticWindowCreatorPatches.GetWindow<TestWindow>;

        // Assert
        act.ShouldThrow<InvalidOperationException>().Message.ShouldBe($"Cannot find window {typeof(TestWindow)}.");
    }

    [Fact]
    public void RegisterWindowHandler_UpdateDictionary() {
        // Arrange
        ProgrammaticWindowCreatorPatches.RegisterWindow<TestWindow>();
        var window = Activator.CreateInstance<TestWindow>();

        // Act
        _StartedField.SetValue(null!, true);
        var handler = (Action<TestWindow>)_RegisteredWindows[typeof(TestWindow)]!;
        handler(window);

        // Assert
        _RegisteredWindows[typeof(TestWindow)].ShouldBe(window);
        ProgrammaticWindowCreatorPatches.GetWindow<TestWindow>().ShouldBe(window);
    }

    [ExcludeFromCodeCoverage]
    private class TestWindow : ProgrammaticWindowBase
    {
        public override Window.Sizing Sizing => Window.Sizing.Fixed(new(1, 2));

        protected override void Populate(UIPanelBuilder builder) {
        }
    }
}
