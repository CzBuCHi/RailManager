using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using JetBrains.Annotations;

namespace RailManager.Wrappers.System.IO.Compression;

/// <summary>
///     Provides a mockable interface for the static methods of <see cref="ZipFile" />.
///     This abstraction enables unit testing of ZIP extraction and archive opening logic
///     without requiring actual file system access or <c>System.IO.Compression</c> dependencies.
/// </summary>
[PublicAPI]
public interface IZipFileStatic
{
    /// <inheritdoc cref="ZipFile.ExtractToDirectory(string, string)" />
    void ExtractToDirectory(string sourceArchiveFileName, string destinationDirectoryName);

    /// <inheritdoc cref="ZipFile.OpenRead(string)" />
    IZipArchive OpenRead(string archiveFileName);
}

[ExcludeFromCodeCoverage]
public sealed class ZipFileStatic : IZipFileStatic
{
    public void ExtractToDirectory(string sourceArchiveFileName, string destinationDirectoryName) =>
        ZipFile.ExtractToDirectory(sourceArchiveFileName, destinationDirectoryName);

    public IZipArchive OpenRead(string archiveFileName) =>
        new ZipArchiveWrapper(ZipFile.OpenRead(archiveFileName)!);
}
