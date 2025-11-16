using System.IO;
using System.Linq;
using MockFileSystem.Entries;
using NSubstitute;
using RailManager.Wrappers.System.IO.Compression;

namespace MockFileSystem.Wrappers;

public sealed class MemoryZipFileStatic(BaseFileSystem fileSystem) : IZipFileStatic
{
    public IZipFileStatic Mock() {
        var mock = Substitute.For<IZipFileStatic>();
        mock.When(o => o.ExtractToDirectory(Arg.Any<string>(), Arg.Any<string>())).Do(o => ExtractToDirectory(o.ArgAt<string>(0), o.ArgAt<string>(1)));
        mock.OpenRead(Arg.Any<string>()).Returns(o => OpenRead(o.Arg<string>()));
        return mock;
    }

    public void ExtractToDirectory(string sourceArchiveFileName, string destinationDirectoryName) {
        sourceArchiveFileName = fileSystem.NormalizePath(sourceArchiveFileName);
        destinationDirectoryName = fileSystem.NormalizePath(destinationDirectoryName);

        var zipEntry = fileSystem.GetEntry<MemoryFileEntry>(sourceArchiveFileName);
        if (zipEntry is not MemoryZipFileEntry zipArchiveEntry) {
            throw new InvalidDataException($"File '{sourceArchiveFileName}' is not ZIP.");
        }

        foreach (var entry in zipArchiveEntry.Content.OrderBy(p => p.Path.Length)) {
            fileSystem.Add(entry with { Path = Path.Combine(destinationDirectoryName, entry.Path) });
        }
    }

    public IZipArchive OpenRead(string archiveFileName) => new MemoryZipArchive(fileSystem.GetEntry<MemoryZipFileEntry>(archiveFileName).Content).Mock();
}
