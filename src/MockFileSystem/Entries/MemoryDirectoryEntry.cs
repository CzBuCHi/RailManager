using System;

namespace MockFileSystem.Entries;

public sealed record MemoryDirectoryEntry(string Path, DateTime LastWriteTime) : MemoryEntry(Path, LastWriteTime);