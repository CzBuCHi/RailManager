using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace RailManagerInstaller.Abstractions;

public interface IDirectoryStatic
{
    void CreateDirectory(string path);
    string GetCurrentDirectory();
    void SetCurrentDirectory(string path);
    bool Exists(string path);
}

[ExcludeFromCodeCoverage]
public sealed class DirectoryStatic : IDirectoryStatic
{
    public void CreateDirectory(string path) => Directory.CreateDirectory(path);

    public string GetCurrentDirectory() => Directory.GetCurrentDirectory();

    public void SetCurrentDirectory(string path) => Directory.SetCurrentDirectory(path);

    public bool Exists(string path) => Directory.Exists(path);
}
