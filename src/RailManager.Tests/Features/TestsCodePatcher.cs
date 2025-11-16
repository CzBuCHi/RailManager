using System;
using System.Linq;
using MockFileSystem;
using Mono.Cecil;
using NSubstitute;
using RailManager.Features;
using RailManager.Features.CodePatchers;
using RailManager.Interfaces;
using RailManager.Interfaces.Markers;
using RailManager.Wrappers.Mono.Cecil;
using Serilog;
using Shouldly;
using IAssemblyDefinition = RailManager.Wrappers.Mono.Cecil.IAssemblyDefinition;
using TypeAttributes = Mono.Cecil.TypeAttributes;

namespace RailManager.Tests.Features;

public sealed class TestsCodePatcher
{
    private const           string   AssemblyPath = @"C:\Current\Mods\DummyMod\DummyMod.dll";
    private static readonly DateTime _OldDate     = new(2000, 1, 2);
    private static readonly DateTime _NewDate     = new(2000, 1, 4);

    private static readonly ModDefinition _ModDefinition = new() {
        Identifier = "DummyMod",
        Name = "Dummy Mod Name",
        BasePath = @"C:\Current\Mods\DummyMod",
        Requires = new() {
            { "SecondMod", new(new(1, 0)) }
        }
    };

    private static PatchModAction Factory(ILogger logger, MemoryFileSystem fileSystem, IAssemblyDefinitionStatic assemblyDefinitionStatic) =>
        (definition, pluginPatchers) => CodePatcher.ApplyPatches(
            logger, assemblyDefinitionStatic,
            fileSystem,
            definition, pluginPatchers ?? CodePatcher.DefaultPluginPatchers
        );

    [Fact]
    public void NoPatches_DoNothing() {
        // Arrange
        var fileSystem = new MemoryFileSystem(@"\Current") {
            { AssemblyPath, "", _OldDate },
            { @"C:\Current\Mods\DummyMod\source.cs", "", _NewDate }
        };

        var logger                 = Substitute.For<ILogger>();
        var readAssemblyDefinition = Substitute.For<IAssemblyDefinitionStatic>();


        var applyPatches = Factory(logger, fileSystem, readAssemblyDefinition);

        // Act
        var actual = applyPatches(_ModDefinition, []);

        // Assert
        actual.ShouldBeTrue();
        logger.ShouldReceiveNoCalls();
    }

    [Fact]
    public void AssemblyLoadFail() {
        // Arrange
        var fileSystem = new MemoryFileSystem(@"\Current") {
            { AssemblyPath, "", _OldDate },
            { @"C:\Current\Mods\DummyMod\source.cs", "", _NewDate }
        };

        var logger                 = Substitute.For<ILogger>();
        var assemblyDefinitionStatic = Substitute.For<IAssemblyDefinitionStatic>();
        assemblyDefinitionStatic.ReadAssembly(Arg.Any<string>(), Arg.Any<ReaderParameters>()).Returns(_ => null);

        var applyPatches = Factory(logger, fileSystem, assemblyDefinitionStatic);

        // Act
        var actual = applyPatches(_ModDefinition, [new(typeof(IHarmonyPlugin), TestPluginPatcher.Factory)]);

        // Assert
        actual.ShouldBeFalse();
        logger.Received().Information("Patching mod {ModId} ...", _ModDefinition.Identifier);
        logger.Received().Error("Failed to load definition for assembly {AssemblyPath} for mod {ModId}", AssemblyPath, _ModDefinition.Identifier);
        logger.Received().Error("Failed to apply patches to assembly {AssemblyPath} for mod {ModId}", AssemblyPath, _ModDefinition.Identifier);
        logger.ShouldReceiveCallCount(3);
    }

    [Fact]
    public void AssemblyReplaceFail() {
        // Arrange
        var assemblyDefinition = BuildAssemblyDefinition();

        var fileSystem = new MemoryFileSystem(@"\Current") {
            { AssemblyPath, "", _OldDate },
            { @"C:\Current\Mods\DummyMod\source.cs", "", _NewDate },
            { @"C:\Current\Mods\SecondMod\SecondMod.dll", "", _OldDate }
        };

        var logger                 = Substitute.For<ILogger>();
        var readAssemblyDefinition = Substitute.For<IAssemblyDefinitionStatic>();
        readAssemblyDefinition.ReadAssembly(Arg.Any<string>(), Arg.Any<ReaderParameters>()).Returns(_ => assemblyDefinition);

        assemblyDefinition.When(o => o.Write(Arg.Any<string>()))
                               .Do(o => {
                                   var tempFilePath = o.Arg<string>();
                                   fileSystem.Add(tempFilePath, "Patched DLL");
                                   fileSystem.LockFile(tempFilePath);
                               });

        var applyPatches = Factory(logger, fileSystem, readAssemblyDefinition);

        string[] expectedDirectories = [
            @"C:\Current\Railroader_Data\Managed",
            @"C:\Current\Mods\SecondMod"
        ];

        // Act
        var actual = applyPatches(_ModDefinition, [new(typeof(IHarmonyPlugin), TestPluginPatcher.Factory)]);

        // Assert
        actual.ShouldBeFalse();
        logger.Received().Information("Patching mod {ModId} ...", "DummyMod");
        logger.Received().Debug("Wrote patched assembly to temporary file {TempPath} for mod {ModId}", @"C:\Current\Mods\DummyMod\DummyMod.patched.dll", "DummyMod");
        logger.Received().Error(Arg.Any<InvalidOperationException>(), "Failed to replace original assembly for mod {ModId}", "DummyMod");
        logger.Received().Error("Failed to apply patches to assembly {AssemblyPath} for mod {ModId}", @"C:\Current\Mods\DummyMod\DummyMod.dll", "DummyMod");

        readAssemblyDefinition.Received(1).ReadAssembly(AssemblyPath,
            Arg.Is<ReaderParameters>(o =>
                o.AssemblyResolver is DefaultAssemblyResolver &&
                ((DefaultAssemblyResolver)o.AssemblyResolver).GetSearchDirectories()!.SequenceEqual(expectedDirectories)
            )
        );
        assemblyDefinition.Received(1).Write(Arg.Any<string>());

        fileSystem.File.Received(1).Delete(AssemblyPath);
        fileSystem.File.Received().Move(@"C:\Current\Mods\DummyMod\DummyMod.patched.dll", AssemblyPath);
    }

    [Fact]
    public void ReturnValidInstances() {
        // Arrange
        var assemblyDefinition = BuildAssemblyDefinition();

        var fileSystem = new MemoryFileSystem(@"\Current") {
            { AssemblyPath, "", _OldDate },
            { @"C:\Current\Mods\DummyMod\source.cs", "", _NewDate },
            { @"C:\Current\Mods\SecondMod\SecondMod.dll", "", _OldDate }
        };

        var logger                 = Substitute.For<ILogger>();
        var readAssemblyDefinition = Substitute.For<IAssemblyDefinitionStatic>();
        readAssemblyDefinition.ReadAssembly(Arg.Any<string>(), Arg.Any<ReaderParameters>()).Returns(_ => assemblyDefinition);
        assemblyDefinition.When(o => o.Write(Arg.Any<string>()))
            .Do(o => fileSystem.Add(o.Arg<string>(), "Patched DLL"));

        var applyPatches = Factory(logger, fileSystem, readAssemblyDefinition);

        string[] expectedDirectories = [
            @"C:\Current\Railroader_Data\Managed",
            @"C:\Current\Mods\SecondMod"
        ];

        // Act
        var actual = applyPatches(_ModDefinition, [new(typeof(IHarmonyPlugin), TestPluginPatcher.Factory)]);

        // Assert
        actual.ShouldBeTrue();
        logger.Received().Information("Patching mod {ModId} ...", _ModDefinition.Identifier);
        logger.Received().Debug("Wrote patched assembly to temporary file {TempPath} for mod {ModId}", Arg.Any<string>(), _ModDefinition.Identifier);
        logger.Received().Information("Patching complete for mod {ModId}", _ModDefinition.Identifier);
        logger.ShouldReceiveCallCount(3);

        readAssemblyDefinition.Received(1).ReadAssembly(AssemblyPath,
            Arg.Is<ReaderParameters>(o =>
                o.AssemblyResolver is DefaultAssemblyResolver &&
                ((DefaultAssemblyResolver)o.AssemblyResolver).GetSearchDirectories()!.SequenceEqual(expectedDirectories)
            )
        );
        assemblyDefinition.Received(1).Write(Arg.Any<string>());
        assemblyDefinition.Received(1).Dispose();

        fileSystem.File.Received(1).Delete(AssemblyPath);
        fileSystem.File.Received(1).Move(@"C:\Current\Mods\DummyMod\DummyMod.patched.dll", AssemblyPath);
    }

    [Fact]
    public void ReturnValidInstances_NoRequires() {
        // Arrange
        ModDefinition modDefinition = new() {
            Identifier = "DummyMod",
            Name = "Dummy Mod Name",
            BasePath = @"C:\Current\Mods\DummyMod"
        };

        var assemblyDefinition = BuildAssemblyDefinition();

        var fileSystem = new MemoryFileSystem(@"\Current") {
            { AssemblyPath, "", _OldDate },
            { @"C:\Current\Mods\DummyMod\source.cs", "", _NewDate },
            { @"C:\Current\Mods\SecondMod\SecondMod.dll", "", _OldDate }
        };

        var logger                 = Substitute.For<ILogger>();
        var readAssemblyDefinition = Substitute.For<IAssemblyDefinitionStatic>();
        readAssemblyDefinition.ReadAssembly(Arg.Any<string>(), Arg.Any<ReaderParameters>()).Returns(_ => assemblyDefinition);

        assemblyDefinition.When(o => o.Write(Arg.Any<string>()))
            .Do(o => fileSystem.Add(o.Arg<string>(), "Patched DLL"));

        var applyPatches = Factory(logger, fileSystem, readAssemblyDefinition);

        string[] expectedDirectories = [
            @"C:\Current\Railroader_Data\Managed"
        ];

        // Act
        var actual = applyPatches(modDefinition, [new(typeof(IHarmonyPlugin), TestPluginPatcher.Factory)]);

        // Assert
        actual.ShouldBeTrue();
        logger.Received().Information("Patching mod {ModId} ...", modDefinition.Identifier);
        logger.Received().Debug("Wrote patched assembly to temporary file {TempPath} for mod {ModId}", Arg.Any<string>(), modDefinition.Identifier);
        logger.Received().Information("Patching complete for mod {ModId}", modDefinition.Identifier);
        logger.ShouldReceiveCallCount(3);

        readAssemblyDefinition.Received(1).ReadAssembly(AssemblyPath,
            Arg.Is<ReaderParameters>(o =>
                o.AssemblyResolver is DefaultAssemblyResolver &&
                ((DefaultAssemblyResolver)o.AssemblyResolver).GetSearchDirectories()!.SequenceEqual(expectedDirectories)
            )
        );
        assemblyDefinition.Received(1).Write(Arg.Any<string>());

        fileSystem.File.Received(1).Delete(AssemblyPath);
        fileSystem.File.Received(1).Move(@"C:\Current\Mods\DummyMod\DummyMod.patched.dll", AssemblyPath);
    }

    [Fact]
    public void ExtraInterface() {
        // Arrange
        var mainModule = ModuleDefinition.CreateModule("Module", ModuleKind.Dll);

        var pluginBaseGeneric = mainModule.ImportReference(typeof(PluginBase<>))!;

        var iExtra = new TypeDefinition("Foo.Bar", "IExtra", TypeAttributes.Interface | TypeAttributes.Public);
        mainModule.Types.Add(iExtra);

        var typeDefinition = new TypeDefinition("Foo.Bar", "FirstPlugin", TypeAttributes.Class | TypeAttributes.Public);

        pluginBaseGeneric.GenericParameters!.Add(new("T", typeDefinition));
        typeDefinition.BaseType = pluginBaseGeneric;

        typeDefinition.Interfaces.Add(new(iExtra));

        mainModule.Types.Add(typeDefinition);

        var assemblyDefinition = Substitute.For<IAssemblyDefinition>();
        assemblyDefinition.MainModule.Returns(mainModule);
        
        var fileSystem = new MemoryFileSystem(@"\Current") {
            { AssemblyPath, "", _OldDate },
            { @"C:\Current\Mods\DummyMod\source.cs", "", _NewDate },
            { @"C:\Current\Mods\SecondMod\SecondMod.dll", "", _OldDate }
        };

        var logger                 = Substitute.For<ILogger>();
        var readAssemblyDefinition = Substitute.For<IAssemblyDefinitionStatic>();
        readAssemblyDefinition.ReadAssembly(Arg.Any<string>(), Arg.Any<ReaderParameters>()).Returns(_ => assemblyDefinition);


        var applyPatches = Factory(logger, fileSystem, readAssemblyDefinition);

        string[] expectedDirectories = [
            @"C:\Current\Railroader_Data\Managed",
            @"C:\Current\Mods\SecondMod"
        ];

        // Act
        var actual = applyPatches(_ModDefinition, [new(typeof(IHarmonyPlugin), TestPluginPatcher.Factory)]);

        // Assert
        actual.ShouldBeTrue();
        logger.Received().Information("Patching mod {ModId} ...", _ModDefinition.Identifier);
        logger.Received().Information("No patches were applied to assembly {AssemblyPath} for mod {ModId}", AssemblyPath, "DummyMod");
        logger.Received().Information("Patching complete for mod {ModId}", _ModDefinition.Identifier);
        logger.ShouldReceiveCallCount(3);

        readAssemblyDefinition.Received(1).ReadAssembly(AssemblyPath,
            Arg.Is<ReaderParameters>(o =>
                o.AssemblyResolver is DefaultAssemblyResolver &&
                ((DefaultAssemblyResolver)o.AssemblyResolver).GetSearchDirectories()!.SequenceEqual(expectedDirectories)
            )
        );
        assemblyDefinition.Received(0).Write(Arg.Any<string>());
    }

    [Fact]
    public void HandleThrowingPatcher1() {
        // Arrange
        var assemblyDefinition = BuildAssemblyDefinition();

        var fileSystem = new MemoryFileSystem(@"\Current") {
            { AssemblyPath, "", _OldDate },
            { @"C:\Current\Mods\DummyMod\source.cs", "", _NewDate },
            { @"C:\Current\Mods\SecondMod\SecondMod.dll", "", _OldDate }
        };

        var logger                 = Substitute.For<ILogger>();
        var readAssemblyDefinition = Substitute.For<IAssemblyDefinitionStatic>();
        readAssemblyDefinition.ReadAssembly(Arg.Any<string>(), Arg.Any<ReaderParameters>()).Returns(_ => assemblyDefinition);


        var applyPatches = Factory(logger, fileSystem, readAssemblyDefinition);

        string[] expectedDirectories = [
            @"C:\Current\Railroader_Data\Managed",
            @"C:\Current\Mods\SecondMod"
        ];

        // Act
        var actual = applyPatches(_ModDefinition, [new(typeof(IHarmonyPlugin), ThrowingPatcher.Factory)]);

        // Assert
        actual.ShouldBeFalse();
        logger.Received().Information("Patching mod {ModId} ...", _ModDefinition.Identifier);
        logger.Received().Error(Arg.Is<Exception>(o => o.Message == "ThrowingPatcher"), "Failed to patch type {TypeName} for mod {ModId}", "Foo.Bar.FirstPlugin", _ModDefinition.Identifier);
        logger.Received().Information("No patches were applied to assembly {AssemblyPath} for mod {ModId}", AssemblyPath, "DummyMod");
        logger.ShouldReceiveCallCount(4);

        readAssemblyDefinition.Received(1).ReadAssembly(AssemblyPath,
            Arg.Is<ReaderParameters>(o =>
                o.AssemblyResolver is DefaultAssemblyResolver &&
                ((DefaultAssemblyResolver)o.AssemblyResolver).GetSearchDirectories()!.SequenceEqual(expectedDirectories)
            )
        );
        assemblyDefinition.Received(0).Write(Arg.Any<string>());
    }

    [Fact]
    public void HandleThrowingPatcher2() {
        // Arrange
        var assemblyDefinition = BuildAssemblyDefinition();

        var fileSystem = new MemoryFileSystem(@"\Current") {
            { AssemblyPath, "", _OldDate },
            { @"C:\Current\Mods\DummyMod\source.cs", "", _NewDate },
            { @"C:\Current\Mods\SecondMod\SecondMod.dll", "", _OldDate }
        };

        var logger                 = Substitute.For<ILogger>();
        var readAssemblyDefinition = Substitute.For<IAssemblyDefinitionStatic>();
        readAssemblyDefinition.ReadAssembly(Arg.Any<string>(), Arg.Any<ReaderParameters>()).Returns(_ => assemblyDefinition);


        var applyPatches = Factory(logger, fileSystem, readAssemblyDefinition);

        string[] expectedDirectories = [
            @"C:\Current\Railroader_Data\Managed",
            @"C:\Current\Mods\SecondMod"
        ];

        // Act
        var actual = applyPatches(_ModDefinition,
            [
                new(typeof(IHarmonyPlugin), TestPluginPatcher.Factory),
                new(typeof(IHarmonyPlugin), ThrowingPatcher.Factory)
            ]
        );

        // Assert
        actual.ShouldBeFalse();
        logger.Received().Information("Patching mod {ModId} ...", _ModDefinition.Identifier);
        logger.Received().Error(Arg.Any<Exception>(), "Failed to patch type {TypeName} for mod {ModId}", "Foo.Bar.FirstPlugin", _ModDefinition.Identifier);
        logger.Received().Information("No patches were applied to assembly {AssemblyPath} for mod {ModId}", AssemblyPath, "DummyMod");
        logger.Received().Error("Failed to apply patches to assembly {AssemblyPath} for mod {ModId}", AssemblyPath, _ModDefinition.Identifier);
        logger.ShouldReceiveCallCount(4);

        readAssemblyDefinition.Received(1).ReadAssembly(AssemblyPath,
            Arg.Is<ReaderParameters>(o =>
                o.AssemblyResolver is DefaultAssemblyResolver &&
                ((DefaultAssemblyResolver)o.AssemblyResolver).GetSearchDirectories()!.SequenceEqual(expectedDirectories)
            )
        );
        assemblyDefinition.Received(0).Write(Arg.Any<string>());
    }
    
    private static IAssemblyDefinition BuildAssemblyDefinition() {
        
        var mainModule = ModuleDefinition.CreateModule("Module", ModuleKind.Dll);

        var pluginBaseGeneric = mainModule.ImportReference(typeof(PluginBase<>))!;
        var iHarmonyPlugin    = mainModule.ImportReference(typeof(IHarmonyPlugin))!;
        
        var typeDefinition = new TypeDefinition("Foo.Bar", "FirstPlugin", TypeAttributes.Class | TypeAttributes.Public);

        pluginBaseGeneric.GenericParameters!.Add(new("T", typeDefinition));
        typeDefinition.BaseType = pluginBaseGeneric;

        typeDefinition.Interfaces.Add(new(iHarmonyPlugin));

        mainModule.Types.Add(typeDefinition);

        var mock = Substitute.For<IAssemblyDefinition>();
        mock.MainModule.Returns(mainModule);
        return mock;
    }

    private static class TestPluginPatcher
    {
        public static TypePatcherDelegate Factory() => (_, _) => true;
    }

    private static class ThrowingPatcher
    {
        public static TypePatcherDelegate Factory() => (_, _) => throw new("ThrowingPatcher");
    }
}
