using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MockFileSystem.Entries;
using NSubstitute;
using RailManager.Wrappers.System.IO;

namespace MockFileSystem.Wrappers;

public sealed class MemoryDirectoryStatic(BaseFileSystem fileSystem) : IDirectoryStatic
{
    public IDirectoryStatic Mock() {
        var mock = Substitute.For<IDirectoryStatic>();
        mock.Exists(Arg.Any<string>()).Returns(o => Exists(o.Arg<string>()));
        mock.EnumerateDirectories(Arg.Any<string>()).Returns(o => EnumerateDirectories(o.Arg<string>()));
        mock.GetCurrentDirectory().Returns(_ => GetCurrentDirectory());
        return mock;
    }

    public bool Exists(string path) => fileSystem.FindEntry<MemoryDirectoryEntry>(path) != null;

    public IEnumerable<string> EnumerateDirectories(string path) =>
        fileSystem.EnumerateEntries(path, "*.*", SearchOption.TopDirectoryOnly).OfType<MemoryDirectoryEntry>().Select(o => o.Path);

    public string GetCurrentDirectory() => fileSystem is MemoryFileSystem memoryFileSystem
        ? memoryFileSystem.CurrentDirectory
        : throw new InvalidOperationException($"Only {typeof(MemoryFileSystem)} supports concept of '{nameof(MemoryFileSystem.CurrentDirectory)}'.");
}