using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using RailManager.Extensions;
using RailManager.Wrappers.HarmonyLib;
using Serilog;

namespace RailManager.Features;

public static class ModLoader
{
    public static Mod[] LoadMods(IReadOnlyList<ModDefinition> modDefinitions) => LoadMods(
        Log.Logger.ForSourceContext(),
        modDefinitions,
        ModDefinitionValidator.Create,
        CodeCompiler.Create(),
        CodePatcher.Create(),
        PluginManager.CreateLoader,
        HarmonyWrapper.CreateWrapper("Railroader.ModManager")
    );

    internal static Mod[] LoadMods(
        ILogger logger,
        IReadOnlyList<ModDefinition> modDefinitions,
        ValidateMods modDefinitionValidator,
        CompileModAction codeCompiler,
        PatchModAction codePatcher,
        PluginLoaderFactory createPluginsDelegateFactory,
        IHarmony harmony
    ) {
        if (modDefinitions.Count == 0) {
            logger.Information("No mods where found.");
            return [];
        }

        logger.Information("Validating mods ...");
        modDefinitions = modDefinitionValidator(modDefinitions);

        if (modDefinitions.Count == 0) {
            logger.Error("Validation error detected. Canceling mod loading.");
            return [];
        }

        var mods = new Mod[modDefinitions.Count];

        for (var i = 0; i < modDefinitions.Count; i++) {
            var definition = modDefinitions[i]!;
            var result     = codeCompiler(definition);
            if (result == CompileModResult.Success) {
                if (!codePatcher(definition)) {
                    result = CompileModResult.Error;
                }
            }

            var assemblyPath = result switch {
                CompileModResult.None or CompileModResult.Error      => null,
                CompileModResult.Success or CompileModResult.Skipped => Path.Combine(definition.BasePath, definition.Identifier + ".dll"),
                _                                                    => throw new ArgumentOutOfRangeException()
            };

            var mod = Mod.Create(logger, definition);
            mod.AssemblyPath = assemblyPath;
            mod.IsValid = result != CompileModResult.Error;

            mods[i] = mod;
        }

        var moddingContext = new ModdingContext(mods);
        logger.Information("Created modding context ...");
        logger.Debug("mods: {mods}", JsonConvert.SerializeObject(moddingContext.Mods));

        var pluginManager = createPluginsDelegateFactory(moddingContext);

        logger.Information("Instantiating plugins ...");
        foreach (var mod in mods.Where(o => o.AssemblyPath != null)) {
            mod.Plugins  = pluginManager(mod).ToArray();
            mod.IsLoaded = true;
        }

        logger.Information("Applying harmony patches ...");
        harmony.PatchAllUncategorized(typeof(ModManager).Assembly);

        return mods;
    }
}
