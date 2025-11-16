using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using _Directory = System.IO.Directory;

namespace RailManager.Wrappers.System.IO;

/// <summary>
///     Provides a mockable interface for the static methods of <see cref="_Directory" />.
///     This abstraction enables unit testing of file system operations involving directories
///     without requiring actual disk access, allowing full control over directory existence,
///     enumeration, and creation behavior in tests.
/// </summary>
[PublicAPI]
public interface IDirectoryStatic
{
    /// <inheritdoc cref="_Directory.Exists(string)" />
    bool Exists(string path);

    /// <inheritdoc cref="_Directory.EnumerateDirectories(string)" />
    IEnumerable<string> EnumerateDirectories(string path);

    /// <inheritdoc cref="_Directory.GetCurrentDirectory()" />
    string GetCurrentDirectory();
}

[ExcludeFromCodeCoverage]
public sealed class DirectoryStatic : IDirectoryStatic
{
    /// <inheritdoc />
    public bool Exists(string path) => _Directory.Exists(path);

    /// <inheritdoc />
    public IEnumerable<string> EnumerateDirectories(string path) => _Directory.EnumerateDirectories(path);

    /// <inheritdoc />
    public string GetCurrentDirectory() => _Directory.GetCurrentDirectory();
}
