using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using GalaSoft.MvvmLight.Messaging;
using Game.Events;
using JetBrains.Annotations;
using Logging;
using RailManager.Extensions;
using RailManager.Features;
using Serilog;
using Serilog.Events;
using UnityEngine;
using ILogger = Serilog.ILogger;

namespace RailManager;

[PublicAPI]
[ExcludeFromCodeCoverage]
public static class ModManager
{
    public static void Bootstrap() {
        try {
            ConfigureLogger();
            Log.Information("RailManager {appVersion} injected into game ...", typeof(ModManager).Assembly.GetName().Version);

            ModExtractor.ExtractAll();
            var modDefinitions = ModDefinitionLoader.LoadDefinitions();
            ConfigureLogger(modDefinitions);

            var mods = ModLoader.LoadMods(modDefinitions);
            var todo = new Todo(Log.Logger.ForSourceContext(), mods, Messenger.Default!);
            todo.Execute();

        } catch (Exception exc) {
            Debug.LogError("Failed to load ModManager ModManager!");
            Debug.LogException(exc);
        }
    }

    private static  LogEventLevel ManagerLogLevel => 
#if DEBUG
        LogEventLevel.Debug;
#else
        LogEventLevel.Information;
#endif

    private static void ConfigureLogger(IReadOnlyList<ModDefinition>? modDefinitions = null) {
        var configuration = LogManager.MakeConfiguration()!;
        var logger        = Log.Logger!;
        try {
            configuration.MinimumLevel.Override("Railroader.ModManager", ManagerLogLevel);

            if (modDefinitions != null) {
                logger.Information("Setting log level for {identifier} to {level}", "Railroader.ModManager", ManagerLogLevel);
                
                foreach (var pair in modDefinitions.Where(o => o.LogLevel != null && o.LogLevel != LogEventLevel.Information)) {
                    logger.Information("Setting log level for {identifier} to {level}", pair.Identifier, pair.LogLevel!.Value);
                    configuration.MinimumLevel.Override(pair.Identifier, pair.LogLevel.Value);
                }
            }

            RemoveUnitySinks(configuration);

            // Configure modded sinks
            configuration.WriteTo.Conditional(o => o.Properties.ContainsKey("SourceContext"),
                o => o.UnityConsole(
                    "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"));
            configuration.WriteTo.Conditional(o => !o.Properties.ContainsKey("SourceContext"),
                o => o.UnityConsole("[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"));

            Log.Logger = configuration.CreateLogger()!;
        } catch (Exception exc) {
            logger.Error(exc, "Failed to configure serilog");
        }
    }

    private static void RemoveUnitySinks(LoggerConfiguration configuration) {
        var field = typeof(LoggerConfiguration).GetField("_logEventSinks",
            BindingFlags.NonPublic | BindingFlags.Instance);
        if (field == null) {
            throw new InvalidOperationException($"Unable to get field {typeof(LoggerConfiguration)}::_logEventSinks");
        }

        var sinks = (IList)field.GetValue(configuration)!;

        foreach (var sink in sinks.OfType<SerilogUnityConsoleEventSink>().ToList()) {
            sinks.Remove(sink);
        }
    }
}

public class Todo(ILogger logger, Mod[] mods, Messenger messenger) : IDisposable
{
    public void Execute() {
        messenger.Register(this, new Action<MapDidLoadEvent>(OnMapDidLoad));
        messenger.Register(this, new Action<MapDidUnloadEvent>(OnMapDidUnload));
    }

    private void OnMapDidLoad(MapDidLoadEvent obj) {
        logger.Information("Enabling plugins ...");
        foreach (var mod in mods.Where(o => o.AssemblyPath != null)) {
            mod.IsEnabled = true;
        }
    }

    private void OnMapDidUnload(MapDidUnloadEvent obj) {
        logger.Information("Disabling plugins ...");
        foreach (var mod in mods.Where(o => o.AssemblyPath != null)) {
            mod.IsEnabled = false;
        }
    }

    public void Dispose() {
        messenger.Unregister(this);
    }
}
