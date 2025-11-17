using System;
using Mono.Cecil;
using Mono.Cecil.Cil;
using RailManagerInstaller.Tests.Utils;
using Shouldly;

namespace RailManagerInstaller.Tests;

[Collection("ModManagerInstaller")]
public sealed class TestsPatcher
{
    private const string ModManagerInterfaces = "RailManager.Interfaces";
    private const string ModManager           = "RailManager";
    private const string ModManagerType       = "RailManager.ModManager";
    private const string LogManagerType       = "Logging.LogManager";
    private const string AssemblyCsharpDll    = "Assembly-CSharp";

    [Fact]
    public void WhenModManagerInterfacesDllNotFound_ThrowsInstallerException() {
        // Arrange
        TestHelper.PrepareAppServices();
        Action[] calls = [
            TestHelper.GetCurrentDirectoryCall,
            TestHelper.FileExistsCall(ModManagerInterfaces, false)
        ];

        // Act
        var act = Patcher.PatchGame;

        // Assert
        act.ShouldThrow<InstallerException>().Message.ShouldBe($"Could not locate file '{ModManagerInterfaces}.dll'.");

        TestHelper.VerifyReceivedCalls(calls);
    }

    [Fact]
    public void WhenModManagerDllNotFound_ThrowsInstallerException() {
        // Arrange
        TestHelper.PrepareAppServices();
        Action[] calls = [
            TestHelper.GetCurrentDirectoryCall,
            TestHelper.FileExistsCall(ModManagerInterfaces, true),
            TestHelper.FileExistsCall(ModManager, false)
        ];

        // Act
        var act = Patcher.PatchGame;

        // Assert
        act.ShouldThrow<InstallerException>().Message.ShouldBe($"Could not locate file '{ModManager}.dll'.");

        TestHelper.VerifyReceivedCalls(calls);
    }

    [Fact]
    public void WhenAssemblyCsharpDllNotFound_ThrowsInstallerException() {
        // Arrange
        TestHelper.PrepareAppServices();
        Action[] calls = [
            TestHelper.GetCurrentDirectoryCall,
            TestHelper.FileExistsCall(ModManagerInterfaces, true),
            TestHelper.FileExistsCall(ModManager, true),
            TestHelper.FileExistsCall(AssemblyCsharpDll, false)
        ];

        // Act
        var act = Patcher.PatchGame;

        // Assert
        act.ShouldThrow<InstallerException>().Message.ShouldBe($"Could not locate file '{AssemblyCsharpDll}.dll'.");

        TestHelper.VerifyReceivedCalls(calls);
    }

    [Fact]
    public void WhenReadModuleThrows_ThrowsInstallerException() {
        // Arrange
        TestHelper.PrepareAppServices();

        Action[] calls = [
            TestHelper.GetCurrentDirectoryCall,
            TestHelper.FileExistsCall(ModManagerInterfaces, true),
            TestHelper.FileExistsCall(ModManager, true),
            TestHelper.FileExistsCall(AssemblyCsharpDll, true),
            TestHelper.ReadModuleCall(AssemblyCsharpDll, new InstallerException("Simulated ReadModule failure"))
        ];

        // Act
        var act = Patcher.PatchGame;

        // Assert
        act.ShouldThrow<InstallerException>().Message!.ShouldContain("Simulated ReadModule failure");

        TestHelper.VerifyReceivedCalls(calls);
    }

    [Fact]
    public void WhenAlreadyPatched_WritesAlreadyPatchedMessageAndExits() {
        // Arrange
        TestHelper.PrepareAppServices();

        var csharpModule = TestHelper.CreateModuleDefinition(AssemblyCsharpDll, ModManager);

        Action[] calls = [
            TestHelper.GetCurrentDirectoryCall,
            TestHelper.FileExistsCall(ModManagerInterfaces, true),
            TestHelper.FileExistsCall(ModManager, true),
            TestHelper.FileExistsCall(AssemblyCsharpDll, true),
            TestHelper.ReadModuleCall(AssemblyCsharpDll, csharpModule),
            () => AppServices.Console.WriteLine("Railroader is already patched.", ConsoleColor.DarkYellow)
        ];

        // Act
        Patcher.PatchGame();

        // Assert
        TestHelper.VerifyReceivedCalls(calls);
    }

    [Fact]
    public void WhenInjectModuleFailsOnModManagerInterfaces_ThrowsInstallerException() {
        // Arrange
        TestHelper.PrepareAppServices();

        var csharpModule = TestHelper.CreateModuleDefinition(AssemblyCsharpDll);

        Action[] calls = [
            TestHelper.GetCurrentDirectoryCall,
            TestHelper.FileExistsCall(ModManagerInterfaces, true),
            TestHelper.FileExistsCall(ModManager, true),
            TestHelper.FileExistsCall(AssemblyCsharpDll, true),
            TestHelper.ReadModuleCall(AssemblyCsharpDll, csharpModule),
            TestHelper.ReadModuleCall(ModManagerInterfaces, new InstallerException("Simulated ReadModule failure"))
        ];

        // Act
        var act = Patcher.PatchGame;

        // Assert
        act.ShouldThrow<InstallerException>().Message.ShouldBe("Simulated ReadModule failure");

        TestHelper.VerifyReceivedCalls(calls);
    }

    [Fact]
    public void WhenInjectModManagerInterfacesSucceeds_AddsAssemblyReference() {
        // Arrange
        TestHelper.PrepareAppServices();

        var csharpModule = TestHelper.CreateModuleDefinition(AssemblyCsharpDll);
        var modManagerInterfacesModule = TestHelper.CreateModuleDefinition(ModManagerInterfaces);

        Action[] calls = [
            TestHelper.GetCurrentDirectoryCall,
            TestHelper.FileExistsCall(ModManagerInterfaces, true),
            TestHelper.FileExistsCall(ModManager, true),
            TestHelper.FileExistsCall(AssemblyCsharpDll, true),
            TestHelper.ReadModuleCall(AssemblyCsharpDll, csharpModule),
            TestHelper.ReadModuleCall(ModManagerInterfaces, modManagerInterfacesModule),
            TestHelper.ReadModuleCall(ModManager, new TestSuicide())
        ];

        // Act
        var act = Patcher.PatchGame;

        // Assert
        act.ShouldThrow<TestSuicide>();

        TestHelper.VerifyReceivedCalls(calls);

        csharpModule.AssemblyReferences.ShouldContain(o => o.Name == ModManagerInterfaces);
    }

    [Fact]
    public void WhenGetTypeLogManagerFails_ThrowsInstallerException() {
        // Arrange
        TestHelper.PrepareAppServices();

        var csharpModule               = TestHelper.CreateModuleDefinition(AssemblyCsharpDll);
        var modManagerInterfacesModule = TestHelper.CreateModuleDefinition(ModManagerInterfaces);
        var modManagerModule = TestHelper.CreateModuleDefinition(ModManager);

        Action[] calls = [
            TestHelper.GetCurrentDirectoryCall,
            TestHelper.FileExistsCall(ModManagerInterfaces, true),
            TestHelper.FileExistsCall(ModManager, true),
            TestHelper.FileExistsCall(AssemblyCsharpDll, true),
            TestHelper.ReadModuleCall(AssemblyCsharpDll, csharpModule),
            TestHelper.ReadModuleCall(ModManagerInterfaces, modManagerInterfacesModule),
            TestHelper.ReadModuleCall(ModManager, modManagerModule),
            TestHelper.GetTypeCall(csharpModule, LogManagerType, (TypeDefinition?)null)
        ];

        // Act
        var act = Patcher.PatchGame;

        // Assert
        act.ShouldThrow<InstallerException>().Message.ShouldBe($"Could not find {LogManagerType} type.");

        TestHelper.VerifyReceivedCalls(calls);
    }

    [Fact]
    public void WhenGetTypeModManagerFails_ThrowsInstallerException() {
        // Arrange
        TestHelper.PrepareAppServices();

        var csharpModule               = TestHelper.CreateModuleDefinition(AssemblyCsharpDll);
        var modManagerInterfacesModule = TestHelper.CreateModuleDefinition(ModManagerInterfaces);
        var modManagerModule           = TestHelper.CreateModuleDefinition(ModManager);
        var logManagerType             = new TypeDefinition("Logging", "LogManager", TypeAttributes.Class | TypeAttributes.Public);

        Action[] calls = [
            TestHelper.GetCurrentDirectoryCall,
            TestHelper.FileExistsCall(ModManagerInterfaces, true),
            TestHelper.FileExistsCall(ModManager, true),
            TestHelper.FileExistsCall(AssemblyCsharpDll, true),
            TestHelper.ReadModuleCall(AssemblyCsharpDll, csharpModule),
            TestHelper.ReadModuleCall(ModManagerInterfaces, modManagerInterfacesModule),
            TestHelper.ReadModuleCall(ModManager, modManagerModule),
            TestHelper.GetTypeCall(csharpModule, "Logging.LogManager", logManagerType),
            TestHelper.GetTypeCall(modManagerModule, "RailManager.ModManager", (TypeDefinition?)null)
        ];

        // Act
        var act = Patcher.PatchGame;

        // Assert
        act.ShouldThrow<InstallerException>().Message.ShouldBe("Could not find RailManager.ModManager type.");

        TestHelper.VerifyReceivedCalls(calls);
    }

    [Fact]
    public void WhenGetMethodBootstrapFails_ThrowsInstallerException() {
        // Arrange
        TestHelper.PrepareAppServices();

        var csharpModule               = TestHelper.CreateModuleDefinition(AssemblyCsharpDll);
        var modManagerInterfacesModule = TestHelper.CreateModuleDefinition(ModManagerInterfaces);
        var modManagerModule           = TestHelper.CreateModuleDefinition(ModManager);
        var logManagerType             = new TypeDefinition("Logging", "LogManager", TypeAttributes.Class | TypeAttributes.Public);
        var modManagerType             = new TypeDefinition("RailManager", "ModManager", TypeAttributes.Class | TypeAttributes.Public);

        Action[] calls = [
            TestHelper.GetCurrentDirectoryCall,
            TestHelper.FileExistsCall(ModManagerInterfaces, true),
            TestHelper.FileExistsCall(ModManager, true),
            TestHelper.FileExistsCall(AssemblyCsharpDll, true),
            TestHelper.ReadModuleCall(AssemblyCsharpDll, csharpModule),
            TestHelper.ReadModuleCall(ModManagerInterfaces, modManagerInterfacesModule),
            TestHelper.ReadModuleCall(ModManager, modManagerModule),
            TestHelper.GetTypeCall(csharpModule, LogManagerType, logManagerType),
            TestHelper.GetTypeCall(modManagerModule, ModManagerType, modManagerType),
            TestHelper.FindMethod(modManagerType, "Bootstrap")
        ];

        // Act
        var act = Patcher.PatchGame;

        // Assert
        act.ShouldThrow<InstallerException>().Message.ShouldBe($"Could not find {ModManagerType}.Bootstrap method.");

        TestHelper.VerifyReceivedCalls(calls);
    }

    [Fact]
    public void WhenGetMethodAwakeFails_ThrowsInstallerException() {
        // Arrange
        TestHelper.PrepareAppServices();

        var csharpModule               = TestHelper.CreateModuleDefinition(AssemblyCsharpDll);
        var modManagerInterfacesModule = TestHelper.CreateModuleDefinition(ModManagerInterfaces);
        var modManagerModule           = TestHelper.CreateModuleDefinition(ModManager);
        var logManagerType             = new TypeDefinition("Logging", "LogManager", TypeAttributes.Class | TypeAttributes.Public);
        var modManagerType             = new TypeDefinition("RailManager", "ModManager", TypeAttributes.Class | TypeAttributes.Public);
        modManagerType.Methods.Add(new("Bootstrap", MethodAttributes.Public | MethodAttributes.Static, modManagerModule.TypeSystem.Void));

        Action[] calls = [
            TestHelper.GetCurrentDirectoryCall,
            TestHelper.FileExistsCall(ModManagerInterfaces, true),
            TestHelper.FileExistsCall(ModManager, true),
            TestHelper.FileExistsCall(AssemblyCsharpDll, true),
            TestHelper.ReadModuleCall(AssemblyCsharpDll, csharpModule),
            TestHelper.ReadModuleCall(ModManagerInterfaces, modManagerInterfacesModule),
            TestHelper.ReadModuleCall(ModManager, modManagerModule),
            TestHelper.GetTypeCall(csharpModule, LogManagerType, logManagerType),
            TestHelper.GetTypeCall(modManagerModule, ModManagerType, modManagerType),
            TestHelper.FindMethod(modManagerType, "Bootstrap"),
            TestHelper.FindMethod(logManagerType, "Awake"),
        ];

        // Act
        var act = Patcher.PatchGame;

        // Assert
        act.ShouldThrow<InstallerException>().Message.ShouldBe($"Could not find {LogManagerType}.Awake method.");

        TestHelper.VerifyReceivedCalls(calls);
    }

    [Fact]
    public void WhenGetMethodMakeConfigurationFails_ThrowsInstallerException() {
        // Arrange
        TestHelper.PrepareAppServices();

        var csharpModule               = TestHelper.CreateModuleDefinition(AssemblyCsharpDll);
        var modManagerInterfacesModule = TestHelper.CreateModuleDefinition(ModManagerInterfaces);
        var modManagerModule           = TestHelper.CreateModuleDefinition(ModManager);
        var logManagerType             = new TypeDefinition("Logging", "LogManager", TypeAttributes.Class | TypeAttributes.Public);
        logManagerType.Methods.Add(new("Awake", MethodAttributes.Public, modManagerModule.TypeSystem.Void));
        var modManagerType             = new TypeDefinition("RailManager", "ModManager", TypeAttributes.Class | TypeAttributes.Public);
        modManagerType.Methods.Add(new("Bootstrap", MethodAttributes.Public | MethodAttributes.Static, modManagerModule.TypeSystem.Void));

        Action[] calls = [
            TestHelper.GetCurrentDirectoryCall,
            TestHelper.FileExistsCall(ModManagerInterfaces, true),
            TestHelper.FileExistsCall(ModManager, true),
            TestHelper.FileExistsCall(AssemblyCsharpDll, true),
            TestHelper.ReadModuleCall(AssemblyCsharpDll, csharpModule),
            TestHelper.ReadModuleCall(ModManagerInterfaces, modManagerInterfacesModule),
            TestHelper.ReadModuleCall(ModManager, modManagerModule),
            TestHelper.GetTypeCall(csharpModule, LogManagerType, logManagerType),
            TestHelper.GetTypeCall(modManagerModule, ModManagerType, modManagerType),
            TestHelper.FindMethod(modManagerType, "Bootstrap"),
            TestHelper.FindMethod(logManagerType, "Awake"),
            TestHelper.FindMethod(logManagerType, "MakeConfiguration"),
        ];

        // Act
        var act = Patcher.PatchGame;

        // Assert
        act.ShouldThrow<InstallerException>().Message.ShouldBe($"Could not find {LogManagerType}.MakeConfiguration method.");

        TestHelper.VerifyReceivedCalls(calls);
    }

    [Fact]
    public void WhenPatchSuccessful() {
        // Arrange
        TestHelper.PrepareAppServices();

        var csharpModule               = TestHelper.CreateModuleDefinition(AssemblyCsharpDll);
        var modManagerInterfacesModule = TestHelper.CreateModuleDefinition(ModManagerInterfaces);
        var modManagerModule           = TestHelper.CreateModuleDefinition(ModManager);
        var logManagerType             = new TypeDefinition("Logging", "LogManager", TypeAttributes.Class | TypeAttributes.Public);
        var awakeMethod                = new MethodDefinition("Awake", MethodAttributes.Public, modManagerModule.TypeSystem.Void);
        var ilProcessor                = awakeMethod.Body.GetILProcessor();
        ilProcessor.Emit(OpCodes.Ret);
        logManagerType.Methods.Add(awakeMethod);
        var makeConfigurationMethod = new MethodDefinition("MakeConfiguration", MethodAttributes.Private, modManagerModule.TypeSystem.Void);
        logManagerType.Methods.Add(makeConfigurationMethod);
        var modManagerType   = new TypeDefinition("RailManager", "ModManager", TypeAttributes.Class | TypeAttributes.Public);
        var bootstrapMethod = new MethodDefinition("Bootstrap", MethodAttributes.Public | MethodAttributes.Static, modManagerModule.TypeSystem.Void);
        modManagerType.Methods.Add(bootstrapMethod);
        var bootstrapMethodReference = new MethodReference("Bootstrap", modManagerModule.TypeSystem.Void, modManagerType);

        Action[] calls = [
            TestHelper.GetCurrentDirectoryCall,
            TestHelper.FileExistsCall(ModManagerInterfaces, true),
            TestHelper.FileExistsCall(ModManager, true),
            TestHelper.FileExistsCall(AssemblyCsharpDll, true),
            TestHelper.ReadModuleCall(AssemblyCsharpDll, csharpModule),
            TestHelper.ReadModuleCall(ModManagerInterfaces, modManagerInterfacesModule),
            TestHelper.ReadModuleCall(ModManager, modManagerModule),
            TestHelper.GetTypeCall(csharpModule, LogManagerType, logManagerType),
            TestHelper.GetTypeCall(modManagerModule, ModManagerType, modManagerType),
            TestHelper.FindMethod(modManagerType, "Bootstrap"),
            TestHelper.FindMethod(logManagerType, "Awake"),
            TestHelper.FindMethod(logManagerType, "MakeConfiguration"),
            TestHelper.ImportReferenceCall(csharpModule, bootstrapMethodReference)
        ];

        // Act
        Patcher.PatchGame();

        // Assert
        TestHelper.VerifyReceivedCalls(calls);


        awakeMethod.Body.Instructions.ShouldContain(o => o.OpCode == OpCodes.Call && o.Operand == bootstrapMethodReference);


        (makeConfigurationMethod.Attributes & MethodAttributes.Public).ShouldBe(MethodAttributes.Public);
    }
}
