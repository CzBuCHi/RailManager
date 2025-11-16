using System;
using System.IO;
using MockFileSystem.Entries;
using NSubstitute;
using RailManager.Wrappers.System.IO.Compression;

namespace MockFileSystem.Wrappers;

public sealed class MemoryZipArchiveEntry(MemoryFileEntry file) : IZipArchiveEntry
{
    public static IZipArchiveEntry? Create(MemoryFileEntry? file) =>
        file != null ? new MemoryZipArchiveEntry(file).Mock() : null;

    private IZipArchiveEntry Mock() {
        var mock = Substitute.For<IZipArchiveEntry>();
        mock.FullName.Returns(_ => FullName);
        mock.Name.Returns(_ => Name);
        mock.Open().Returns(_ => Open());
        return mock;
    }

    public string FullName => file.Path;
    public string Name     => Path.GetFileName(FullName);

    public Stream Open() =>
        file is MemoryBinaryFileEntry binaryFile
            ? new MemoryStream(binaryFile.Content)
            : throw new NotSupportedException($"Not supported file type: {file.GetType().Name}");
}