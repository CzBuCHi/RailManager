using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using MockFileSystem.Entries;
using NSubstitute;
using RailManager.Wrappers.System.IO.Compression;

namespace MockFileSystem.Wrappers;

public sealed class MemoryZipArchive(ZipFileSystem fileSystem) : IZipArchive
{
    public IZipArchive Mock() {
        var mock = Substitute.For<IZipArchive>();
        mock.Entries.Returns(_ => Entries);
        mock.GetEntry(Arg.Any<string>()).Returns(o => GetEntry(o.Arg<string>()));
        return mock;
    }

    public IReadOnlyCollection<IZipArchiveEntry> Entries =>
        fileSystem.OfType<MemoryFileEntry>().Select(MemoryZipArchiveEntry.Create).ToArray()!;

    public IZipArchiveEntry? GetEntry(string entryName) => MemoryZipArchiveEntry.Create(fileSystem.FindEntry<MemoryFileEntry>(entryName));

    [ExcludeFromCodeCoverage]
    public void Dispose() {
    }
}
