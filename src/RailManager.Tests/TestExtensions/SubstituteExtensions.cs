using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using NSubstitute.Core;
using NSubstitute.Core.Arguments;
using Shouldly;

// ReSharper disable once CheckNamespace
namespace NSubstitute;

[ExcludeFromCodeCoverage]
public static class SubstituteExtensions {
    public static void ShouldReceiveNoCalls<T>(this T substitute) where T : class =>
        substitute.ReceivedCalls().ShouldBeEmpty();

    public static void ShouldReceiveCallCount<T>(this T substitute, int count) where T : class =>
        substitute.ReceivedCalls().Count().ShouldBe(count);
}

[ExcludeFromCodeCoverage]
public static class StrictExtensions
{
    public static T Strict<T>(this T obj, Action<T> action) {
        // let user configure obj ...
        action.Invoke(obj);

        // then add configuration to all unconfigured methods so they throw ...
        var callResults     = obj.CallRouterProvider().GetCallRouter()!.SubstituteState().CallResults!;
        var results         = GetResults(callResults);

        IReturn notMocked = new ReturnValueFromFunc<object>(_ => throw new NotImplementedException("Not mocked"));

        var methods = typeof(T).GetMethods(BindingFlags.Instance | BindingFlags.Public);
        var getMethods = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(o => o.CanRead && o.GetMethod != null)
            .Select(o => o.GetMethod);
        var setMethods = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(o => o.CanWrite && o.SetMethod != null)
            .Select(o => o.SetMethod);

        methods = methods.Concat(getMethods).Concat(setMethods).ToArray();

        foreach (var method in methods) {
            var argumentSpecifications = BuildArgumentSpecification<T>(method);
            var call = BuildCall(obj, method, argumentSpecifications);

            if (!results.Any(o => o.IsResultFor(call))) {
                callResults.SetResult(new CallSpecification(method, argumentSpecifications), notMocked);
            }
        }

        return obj;
    }

    private static ICallRouterProvider CallRouterProvider<T>(this T obj) =>
        (ICallRouterProvider)obj!.GetType()
            .GetField("__mixin_NSubstitute_Core_ICallRouterProvider", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(obj)!;

    private static ISubstituteState SubstituteState(this ICallRouter callRouter) =>
        (ISubstituteState)callRouter
            .GetType()
            .GetField("<substituteState>P", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(callRouter)!;

    private static ICollection GetResultsCollection(this ICallResults callResults) =>
        (ICollection)typeof(CallResults)
            .GetField("_results", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(callResults)!;

    private static ResultForCallSpecWrapper[] GetResults(this ICallResults callResults) =>
        callResults.GetResultsCollection()
            .Cast<object>()
            .Select(o => new ResultForCallSpecWrapper(o))
            .ToArray();

    private static IArgumentSpecification[] BuildArgumentSpecification<T>(MethodInfo method) =>
        method.GetParameters()
            .Select(IArgumentSpecification (o) => new ArgumentSpecification(typeof(T), new AnyArgumentMatcher(o.ParameterType)))
            .ToArray();

    private static object?[] GetArguments(MethodInfo method) =>
        method.GetParameters()
            .Select(o => o.ParameterType.IsValueType ? Activator.CreateInstance(o.ParameterType) : null)
            .ToArray();

    private static Call BuildCall<T>(T obj, MethodInfo method, IArgumentSpecification[] argumentSpecifications) =>
        new(method, GetArguments(method), obj!, argumentSpecifications, null);

    private class ResultForCallSpecWrapper(object instance) {
        private static readonly Type       _Type              = typeof(CallResults).Assembly.GetType("NSubstitute.Core.CallResults+ResultForCallSpec")!;
        private static readonly MethodInfo _IsResultForMethod = _Type.GetMethod("IsResultFor")!;

        public bool IsResultFor(ICall call) => (bool)_IsResultForMethod.Invoke(instance, [call]);
    }
}

[ExcludeFromCodeCoverage]
public static class SubstituteDebugExtensions {
    
    public static string PrintReceivedCalls<T>(this T substitute, string callerName) where T : class {
        var sb = new StringBuilder();
        foreach (var call in substitute.ReceivedCalls()) {
            PrintCall(call);
        }

        return sb.ToString();

        void PrintCall(ICall call) {
            var method = call.GetMethodInfo()!;
            sb.Append(callerName).Append(".Received().");
            sb.Append(method.Name);
            sb.Append('(');

            var args  = call.GetArguments()!;
            var first = true;
            foreach (var arg in args) {
                if (!first) {
                    sb.Append(", ");
                }

                first = false;

                sb.Append(ArgToString(arg));
            }

            sb.AppendLine(");");
        }
    }

    private static string ArgToString(object? arg) {
        if (arg is null) {
            return "null";
        }

        var type = arg.GetType();

        // --------------------------------------------------------------------
        // 1. Primitive / well-known value types
        // --------------------------------------------------------------------
        if (type.IsPrimitive || type == typeof(decimal) || type == typeof(DateTime) || type == typeof(Guid)) {
            return PrimitiveToSource(arg, type);
        }

        // --------------------------------------------------------------------
        // 2. String – choose the best literal style
        // --------------------------------------------------------------------
        if (arg is string s) {
            return s.Contains("\\") ? $"@\"{s}\"" : $"\"{EscapeNormal(s)}\"";
        }

        // --------------------------------------------------------------------
        // 3. Char
        // --------------------------------------------------------------------
        if (arg is char c) {
            return CharToSource(c);
        }

        // --------------------------------------------------------------------
        // 4. Enumerable / Array (including multi-dimensional & jagged)
        // --------------------------------------------------------------------
        if (arg is IEnumerable enumerable && arg is not string) {
            return EnumerableToSource(enumerable);
        }

        // --------------------------------------------------------------------
        // 5. Fallback – Arg.Any<T>() style (for complex objects)
        // --------------------------------------------------------------------
        return $"Arg.Any<{type.Name}>()";
    }

    private static string PrimitiveToSource(object value, Type type) {
        // bool
        if (type == typeof(bool)) {
            return (bool)value ? "true" : "false";
        }

        // No suffix for byte, sbyte, short, ushort
        if (type == typeof(byte) || type == typeof(sbyte) || type == typeof(short) || type == typeof(ushort) || type == typeof(int)) {
            return value.ToString();
        }

        // Suffix for uint, long, ulong
        if (type == typeof(uint)) {
            return value + "u";
        }

        if (type == typeof(long)) {
            return value + "L";
        }

        if (type == typeof(ulong)) {
            return value + "UL";
        }

        // float, double, decimal
        if (type == typeof(float)) {
            return value + "f";
        }

        if (type == typeof(double)) {
            return value.ToString();
        }

        if (type == typeof(decimal)) {
            return value + "m";
        }

        // DateTime / Guid
        if (value is DateTime dt) {
            return $"DateTime.Parse(\"{dt:o}\")";
        }

        if (value is Guid g) {
            return $"new Guid(\"{g:D}\")";
        }

        // Fallback
        return value.ToString();
    }

    private static string CharToSource(char c) {
        return c switch {
            '\'' => @"'\''",
            '\\' => @"'\\'",
            '\0' => @"'\0'",
            '\a' => @"'\a'",
            '\b' => @"'\b'",
            '\f' => @"'\f'",
            '\n' => @"'\n'",
            '\r' => @"'\r'",
            '\t' => @"'\t'",
            '\v' => @"'\v'",
            _ => c < 32 || c > 126
                ? $"\'\\u{(int)c:x4}\'"
                : $"'{c}'"
        };
    }
    
    private static string EscapeNormal(string s)
        => s.Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\0", "\\0")
            .Replace("\a", "\\a")
            .Replace("\b", "\\b")
            .Replace("\f", "\\f")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t")
            .Replace("\v", "\\v");

    private static string EnumerableToSource(IEnumerable enumerable) {
        var elements = new List<string>();

        foreach (var item in enumerable) {
            elements.Add(ArgToString(item));
        }

        // Detect array type to emit the proper cast
        var arr    = enumerable as Array;
        var prefix = "new[]";

        if (arr != null) {
            var rank        = arr.Rank;
            var elementType = arr.GetType().GetElementType()!;

            if (rank == 1) {
                prefix = $"new {elementType.Name}[]";
            } else {
                // Multi-dimensional: new int[2,3] { {…}, {…} }
                var dims = string.Join(",", Enumerable.Range(0, rank).Select(o => arr.GetLength(o)));
                prefix = $"new {elementType.Name}[{dims}]";
            }
        }

        // For non-array IEnumerable we just emit new[] { … }
        var inner = string.Join(", ", elements);
        return $"{prefix} {{ {inner} }}";
    }
}

