using System;

namespace MockFileSystem.Entries;

public abstract record MemoryEntry(string Path, DateTime LastWriteTime)
{
    public static DateTime DefaultLastWriteTime = new(2000, 1, 2);
}