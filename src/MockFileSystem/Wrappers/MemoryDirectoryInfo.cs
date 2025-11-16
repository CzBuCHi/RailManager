using System.Collections.Generic;
using System.IO;
using System.Linq;
using MockFileSystem.Entries;
using NSubstitute;
using RailManager.Wrappers.System.IO;

namespace MockFileSystem.Wrappers;

public sealed class MemoryDirectoryInfo(BaseFileSystem fileSystem, string path) : IDirectoryInfo
{
    public IDirectoryInfo Mock() {
        var mock = Substitute.For<IDirectoryInfo>();
        mock.EnumerateFiles(Arg.Any<string>(), Arg.Any<SearchOption>()).Returns(o => EnumerateFiles(o.Arg<string>(), o.Arg<SearchOption>()));
        return mock;
    }

    public IEnumerable<IFileInfo> EnumerateFiles(string searchPattern, SearchOption searchOption = SearchOption.TopDirectoryOnly) =>
        fileSystem.EnumerateEntries(path, searchPattern, searchOption).OfType<MemoryFileEntry>().Select(o => fileSystem.FileInfo(o.Path));
}