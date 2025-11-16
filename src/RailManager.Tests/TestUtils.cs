using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using Mono.CSharp;
using NSubstitute;
using RailManager.Features;
using RailManager.Interfaces;
using Serilog;
using IAssemblyDefinition = RailManager.Wrappers.Mono.Cecil.IAssemblyDefinition;

namespace RailManager.Tests;

[PublicAPI]
[ExcludeFromCodeCoverage]
public static class TestUtils
{
    private const string GameDir = @"c:\projects\RailManager\game\";

    public static IAssemblyDefinition BuildAssemblyDefinition(string source) {
        var outputPath = Path.GetTempFileName();
        File.Delete(outputPath);
        Directory.CreateDirectory(outputPath);

        var logger = Substitute.For<ILogger>();

        Directory.CreateDirectory(outputPath);
        var sourcePath   = Path.Combine(outputPath, "source.cs");
        var assemblyPath = Path.Combine(outputPath, "output.dll");

        File.WriteAllText(sourcePath, source);

        var sources = new[] { sourcePath };
        var references = new[] {
                "Assembly-CSharp",
                "0Harmony",
                typeof(IPlugin).Assembly.GetName().Name,
                "Serilog",
                "UnityEngine.CoreModule"
            }
            .Select(o => Path.Combine(GameDir, "Railroader_Data", "Managed", o + ".dll"))
            .ToList();

        references.Add(typeof(DateTime).Assembly.Location);
        references.Add(typeof(TestUtils).Assembly.Location);
        
        var result = AssemblyCompiler.Compile(CompilerCallableEntryPoint.InvokeCompiler, logger, assemblyPath, sources, references, []);
        if (!result) {
            throw new InvalidOperationException("Failed to compile source:\r\n" + logger.PrintReceivedCalls("logger"));
        }

        var assemblyDefinition = Mono.Cecil.AssemblyDefinition.ReadAssembly(assemblyPath)!;

        return Substitute.For<IAssemblyDefinition>().Strict(o => {
            o.MainModule.Returns(assemblyDefinition.MainModule);
        });
    }

    public static Assembly BuildAssembly(string source, string[]? references = null) {
        var settings = new CompilerSettings {
            Target = Target.Library,
            Optimize = true,
            Platform = Platform.AnyCPU,
            ShowFullPaths = true
        };
        settings.ReferencesLookupPaths.Add(Directory.GetCurrentDirectory());
        settings.AssemblyReferences.AddRange([
            "Assembly-CSharp",
            "0Harmony",
            typeof(IMod).Assembly.GetName().Name,
            "Serilog",
            "UnityEngine.CoreModule"
        ]);

        if (references != null) {
            settings.AssemblyReferences.AddRange(references);
        }


        var printer = new SimpleReportPrinter();
        var context = new CompilerContext(settings, printer);
        var eval    = new Evaluator(context);

        eval.Compile(source + " interface __AssemblyMarker { } ");
        if (context.Report.Errors > 0) {
            throw new($"Compilation error: {printer.Messages}");
        }

        return (Assembly)eval.Evaluate(" typeof(__AssemblyMarker).Assembly ")!;
    }

    private sealed class SimpleReportPrinter : ReportPrinter
    {
        private readonly List<string> _Messages = new();

        public string Messages => string.Join("\r\n", _Messages);

        public override void Print(AbstractMessage msg, bool showFullPath) {
            base.Print(msg, showFullPath);

            var sb = new StringBuilder();
            using (var output = new StringWriter(sb)) {
                Print(msg, output, showFullPath);
            }

            _Messages.Add(sb.ToString());
        }
    }
}
