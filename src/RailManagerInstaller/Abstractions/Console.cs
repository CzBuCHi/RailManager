using System;
using System.Diagnostics.CodeAnalysis;

namespace RailManagerInstaller.Abstractions;

public interface IConsoleStatic
{
    void Write(object value);
    void WriteLine(object value, ConsoleColor? color = null);
    void SetTitle(string title);
    void ReadKey();
}

[ExcludeFromCodeCoverage]
public sealed class ConsoleStatic : IConsoleStatic
{
    public void Write(object value) => System.Console.Write(value);

    public void WriteLine(object value, ConsoleColor? color = null) {
        if (color != null) {
            System.Console.ForegroundColor = color.Value;
        }

        System.Console.Error.WriteLine(value);

        if (color != null) {
            System.Console.ResetColor();
        }
    }

    public void SetTitle(string title) => System.Console.Title = title;

    public void ReadKey() => System.Console.ReadKey();
}
