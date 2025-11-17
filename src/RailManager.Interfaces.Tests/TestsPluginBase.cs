using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using NSubstitute;
using Shouldly;

namespace RailManager.Interfaces.Tests;

public sealed class TestsPluginBase : IAsyncLifetime
{
    private IModdingContext _ModdingContext = null!;
    private IMod            _Mod            = null!;
    private TestPlugin      _Sut            = null!;

    public Task InitializeAsync() {
        _ModdingContext = Substitute.For<IModdingContext>();
        _Mod = Substitute.For<IMod>();
        _Sut = new(_ModdingContext, _Mod);
        return Task.CompletedTask;
    }

    public Task DisposeAsync() {
        TestPlugin.Cleanup();
        return Task.CompletedTask;
    }

    [Fact]
    public void CallsOnEnableCorrectly() {
        // Arrange
        _Sut.SetIsEnabled(false);

        // Act
        _Sut.IsEnabled = true;
        _Sut.IsEnabled = true;

        // Assert
        _Sut.IsEnabled.ShouldBeTrue();
        _Sut.IsEnabledChanges.ShouldBeEquivalentTo(new List<bool> { true });
    }

    [Fact]
    public void CallsOnDisableCorrectly() {
        // Arrange
        _Sut.SetIsEnabled(true);

        // Act
        _Sut.IsEnabled = false;
        _Sut.IsEnabled = false;

        // Assert
        _Sut.IsEnabled.ShouldBeFalse();
        _Sut.IsEnabledChanges.ShouldBeEquivalentTo(new List<bool> { false });
    }

    public sealed class TestPlugin : PluginBase
    {
        public static TestPlugin? Instance { get; private set; }

        public TestPlugin(IModdingContext moddingContext, IMod mod) : base(moddingContext, mod) => Instance = this;

        private static readonly FieldInfo _IsEnabled = typeof(PluginBase).GetField("_IsEnabled", BindingFlags.Instance | BindingFlags.NonPublic)!;


        public void SetIsEnabled(bool value) => _IsEnabled.SetValue(this, value);

        public static void Cleanup() => Instance = null;

        public readonly List<bool> IsEnabledChanges = new();

        protected override void OnIsEnabledChanged() {
            base.OnIsEnabledChanged();
            IsEnabledChanges.Add(IsEnabled);
        }
    }
}
