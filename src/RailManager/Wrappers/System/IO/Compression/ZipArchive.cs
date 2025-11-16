using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Linq;
using JetBrains.Annotations;

namespace RailManager.Wrappers.System.IO.Compression;

/// <summary>
///     Defines a mockable abstraction over <see cref="ZipArchive" /> for use in mod management systems.
///     This interface exposes only the essential read-only operations needed to inspect archive contents,
///     allowing full control in unit tests without requiring actual ZIP files or <c>System.IO.Compression</c>
///     dependencies.
/// </summary>
[PublicAPI]
public interface IZipArchive : IDisposable
{
    /// <inheritdoc cref="ZipArchive.Entries" />
    IReadOnlyCollection<IZipArchiveEntry> Entries { get; }

    /// <inheritdoc cref="ZipArchive.GetEntry(string)" />
    IZipArchiveEntry? GetEntry(string entryName);
}

[ExcludeFromCodeCoverage]
public sealed class ZipArchiveWrapper(ZipArchive archive) : IZipArchive
{
    /// <summary>
    ///     Creates a new <see cref="IZipArchive" /> wrapper around an existing <see cref="ZipArchive" /> instance.
    /// </summary>
    /// <param name="archive">The existing ZipArchive instance to wrap.</param>
    /// <returns>An <see cref="IZipArchive" /> implementation that delegates to the provided instance.</returns>
    public static IZipArchive? CreateWrapper(ZipArchive? archive) =>
        archive != null ? new ZipArchiveWrapper(archive) : null;

    /// <inheritdoc />
    public IReadOnlyCollection<IZipArchiveEntry> Entries =>
        archive.Entries.Select(ZipArchiveEntryWrapper.CreateWrapper).Cast<IZipArchiveEntry>().ToList().AsReadOnly();

    /// <inheritdoc />
    public IZipArchiveEntry? GetEntry(string entryName) => ZipArchiveEntryWrapper.CreateWrapper(archive.GetEntry(entryName));

    /// <inheritdoc />
    public void Dispose() => archive.Dispose();
}
