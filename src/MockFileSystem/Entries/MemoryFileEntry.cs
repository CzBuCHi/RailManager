using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace MockFileSystem.Entries;

public abstract record MemoryFileEntry(string Path, DateTime LastWriteTime, bool Locked = false) : MemoryEntry(Path, LastWriteTime);

[ExcludeFromCodeCoverage]
public sealed record MemoryBinaryFileEntry(string Path, DateTime LastWriteTime, byte[] Content) : MemoryFileEntry(Path, LastWriteTime)
{
    public static byte[] GetBytes(string content) => Encoding.UTF8.GetBytes(content);
    public static string GetString(byte[] content) => Encoding.UTF8.GetString(content);

    public MemoryBinaryFileEntry(string Path, DateTime LastWriteTime, string Content)
        : this(Path, LastWriteTime, GetBytes(Content)) {
    }

    public string StringContent => GetString(Content);
}

public sealed record MemoryReadFailFileEntry(string Path, DateTime LastWriteTime, Exception ReadException) : MemoryFileEntry(Path, LastWriteTime);

public sealed record MemoryZipFileEntry(string Path, DateTime LastWriteTime, ZipFileSystem Content) : MemoryFileEntry(Path, LastWriteTime);