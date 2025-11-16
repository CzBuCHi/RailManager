using System;
using MockFileSystem.Entries;
using NSubstitute;
using RailManager.Wrappers.System.IO;

namespace MockFileSystem.Wrappers;

public sealed class MemoryFileInfo(BaseFileSystem fileSystem, string path) : IFileInfo
{
    public IFileInfo Mock() {
        var mock = Substitute.For<IFileInfo>();
        mock.LastWriteTime.Returns(_ => LastWriteTime);
        mock.FullName.Returns(_ => FullName);
        mock.When(o => o.MoveTo(Arg.Any<string>())).Do(o => MoveTo(o.Arg<string>()));
        return mock;
    }

    public DateTime LastWriteTime => fileSystem.GetEntry<MemoryFileEntry>(FullName).LastWriteTime;
    public string   FullName      { get; private set; } = fileSystem.NormalizePath(path);

    public void MoveTo(string destFileName) {
        destFileName = fileSystem.NormalizePath(destFileName);
        fileSystem.File.Move(FullName, destFileName);
        FullName = destFileName;
    }
}