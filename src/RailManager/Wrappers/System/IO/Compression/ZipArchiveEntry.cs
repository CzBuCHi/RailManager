using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using JetBrains.Annotations;

namespace RailManager.Wrappers.System.IO.Compression;

/// <summary>
///     Defines a mockable abstraction over <see cref="ZipArchiveEntry" /> for use in mod archive processing.
///     This interface provides access to entry metadata and stream operations without coupling
///     to the concrete <c>System.IO.Compression</c> implementation, enabling full test isolation.
/// </summary>
[PublicAPI]
public interface IZipArchiveEntry
{
    /// <inheritdoc cref="ZipArchiveEntry.FullName" />
    string FullName { get; }

    /// <inheritdoc cref="ZipArchiveEntry.Name" />
    string Name { get; }

    /// <inheritdoc cref="ZipArchiveEntry.Open()" />
    Stream Open();
}

[ExcludeFromCodeCoverage]
public sealed class ZipArchiveEntryWrapper(ZipArchiveEntry entry) : IZipArchiveEntry
{
    /// <summary>
    ///     Creates a new <see cref="IZipArchiveEntry" /> wrapper around an existing <see cref="ZipArchiveEntry" /> instance.
    /// </summary>
    /// <param name="archiveEntry">The existing ZipArchiveEntry instance to wrap.</param>
    /// <returns>An <see cref="IZipArchiveEntry" /> implementation that delegates to the provided instance.</returns>
    public static IZipArchiveEntry? CreateWrapper(ZipArchiveEntry? archiveEntry) =>
        archiveEntry != null ? new ZipArchiveEntryWrapper(archiveEntry) : null;

    /// <inheritdoc />
    public string FullName => entry.FullName;

    /// <inheritdoc />
    public string Name => entry.Name;

    /// <inheritdoc />
    public Stream Open() => entry.Open();
}
