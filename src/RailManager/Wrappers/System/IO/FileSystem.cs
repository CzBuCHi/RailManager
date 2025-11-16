using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using RailManager.Wrappers.System.IO.Compression;

namespace RailManager.Wrappers.System.IO;

/// <summary>
///     A unified facade that aggregates all file system abstractions (Directory, File, ZIP, and info objects)
///     into a single injectable service. This reduces constructor parameter count in consumers while maintaining
///     full mockability for unit testing.
/// </summary>
[PublicAPI]
public interface IFileSystem
{
    /// <summary>
    ///     Gets the abstraction over <see cref="Directory" /> static methods.
    /// </summary>
    IDirectoryStatic Directory { get; }

    /// <summary>
    ///     Gets the abstraction over <see cref="File" /> static methods.
    /// </summary>
    IFileStatic File { get; }

    /// <summary>
    ///     Gets the abstraction over <see cref="ZipFile" /> static methods.
    /// </summary>
    IZipFileStatic ZipFile { get; }

    /// <summary>
    ///     Creates a wrapped <see cref="DirectoryInfo" /> instance for the specified path.
    /// </summary>
    /// <param name="path">The directory path.</param>
    /// <returns>A mockable <see cref="IDirectoryInfo" /> wrapper.</returns>
    IDirectoryInfo DirectoryInfo(string path);

    /// <summary>
    ///     Creates a wrapped <see cref="FileInfo" /> instance for the specified path.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <returns>A mockable <see cref="IFileInfo" /> wrapper.</returns>
    IFileInfo FileInfo(string path);
}

/// <summary>
///     Default implementation of <see cref="IFileSystem" /> using real .NET I/O wrappers.
///     Marked for exclusion from code coverage — contains no logic, only composition.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class FileSystem : IFileSystem
{
    /// <summary>
    ///     Singleton instance for default (real) file system behavior.
    ///     Use in production or integration tests.
    /// </summary>
    public static readonly IFileSystem Instance = new FileSystem();

    /// <inheritdoc />
    public IDirectoryStatic Directory { get; } = new DirectoryStatic();

    /// <inheritdoc />
    public IFileStatic File { get; } = new FileStatic();

    /// <inheritdoc />
    public IZipFileStatic ZipFile { get; } = new ZipFileStatic();

    /// <inheritdoc />
    public IDirectoryInfo DirectoryInfo(string path) => new DirectoryInfoWrapper(new(path));

    /// <inheritdoc />
    public IFileInfo FileInfo(string path) => new FileInfoWrapper(new(path));
}
