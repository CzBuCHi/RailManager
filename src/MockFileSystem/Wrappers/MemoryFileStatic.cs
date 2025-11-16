using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MockFileSystem.Entries;
using MockFileSystem.Utility;
using NSubstitute;
using RailManager.Wrappers.System.IO;

namespace MockFileSystem.Wrappers;

public sealed class MemoryFileStatic(BaseFileSystem fileSystem) : IFileStatic
{
    public IFileStatic Mock() {
        var mock = Substitute.For<IFileStatic>();
        mock.Exists(Arg.Any<string>()).Returns(o => Exists(o.Arg<string>()));
        mock.ReadAllText(Arg.Any<string>()).Returns(o => ReadAllText(o.Arg<string>()));
        mock.When(o => o.WriteAllText(Arg.Any<string>(), Arg.Any<string>())).Do(o => WriteAllText(o.ArgAt<string>(0), o.ArgAt<string>(1)));
        mock.GetLastWriteTime(Arg.Any<string>()).Returns(o => GetLastWriteTime(o.Arg<string>()));
        mock.When(o => o.Delete(Arg.Any<string>())).Do(o => Delete(o.Arg<string>()));
        mock.When(o => o.Move(Arg.Any<string>(), Arg.Any<string>())).Do(o => Move(o.ArgAt<string>(0), o.ArgAt<string>(1)));
        mock.Create(Arg.Any<string>()).Returns(o => Create(o.Arg<string>()));
        return mock;
    }

    public bool Exists(string path) => fileSystem.FindEntry<MemoryFileEntry>(path) != null;

    public string ReadAllText(string path) {
        var entry = fileSystem.GetEntry<MemoryFileEntry>(path);
        return entry switch {
                   MemoryReadFailFileEntry readFailFileEntry => throw readFailFileEntry.ReadException,
                   MemoryBinaryFileEntry binaryFile          => binaryFile.StringContent,
                   _                                         => throw new NotSupportedException($"Not supported file type: {entry.GetType().Name}")
               };
    }

    public void WriteAllText(string path, string content) {
        if (fileSystem.FindEntry<MemoryBinaryFileEntry>(path) == null) {
            fileSystem.Add(path, content);
        } else {
            fileSystem.UpdateEntry<MemoryBinaryFileEntry>(path, o => o with { Content = MemoryBinaryFileEntry.GetBytes(content) });
        }
    }

    public DateTime GetLastWriteTime(string path) => fileSystem.GetEntry<MemoryFileEntry>(path).LastWriteTime;

    public void Delete(string path) => fileSystem.DeleteEntry<MemoryFileEntry>(path);

    public void Move(string sourceFileName, string destFileName) {
        sourceFileName = fileSystem.NormalizePath(sourceFileName);
        destFileName = fileSystem.NormalizePath(destFileName);

        lock (fileSystem) {
            var source = fileSystem.GetEntry<MemoryFileEntry>(sourceFileName);
            if (source.Locked) {
                throw new InvalidOperationException($"File at '{sourceFileName}' is locked.");
            }

            if (fileSystem.FindEntry<MemoryFileEntry>(destFileName) != null) {
                throw new InvalidOperationException($"Destination '{destFileName}' exists.");
            }

            fileSystem.UpdateEntry<MemoryFileEntry>(sourceFileName, o => o with { Path = destFileName });
        }
    }

    public Stream Create(string path) {
        path = fileSystem.NormalizePath(path);

        lock (fileSystem) {
            fileSystem.Add(path, Array.Empty<byte>());

            var data = new List<byte>();
            return new MemoryFileStream((buffer, offset, count) => data.AddRange(buffer.Skip(offset).Take(count)),
                () => fileSystem.UpdateEntry<MemoryBinaryFileEntry>(path, o => o with { Content = data.ToArray() }));
        }
    }
}
