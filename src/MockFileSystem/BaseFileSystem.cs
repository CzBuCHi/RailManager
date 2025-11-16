using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using MockFileSystem.Entries;
using MockFileSystem.Utility;
using MockFileSystem.Wrappers;
using RailManager.Wrappers.System.IO;
using RailManager.Wrappers.System.IO.Compression;

namespace MockFileSystem;

[PublicAPI]
public abstract class BaseFileSystem : IFileSystem, IEnumerable<MemoryEntry>
{
    private readonly EntryDictionary _Entries = new();

    protected BaseFileSystem() {
        Directory = new MemoryDirectoryStatic(this).Mock();
        File = new MemoryFileStatic(this).Mock();
        ZipFile = new MemoryZipFileStatic(this).Mock();
    }

    public IDirectoryStatic Directory { get; }
    public IFileStatic      File      { get; }
    public IZipFileStatic   ZipFile   { get; }
    public IDirectoryInfo DirectoryInfo(string path) => new MemoryDirectoryInfo(this, path).Mock();
    public IFileInfo FileInfo(string path) => new MemoryFileInfo(this, path).Mock();

    public void Add(string folderPath, DateTime? lastWriteTime = null) =>
        Add(new MemoryDirectoryEntry(folderPath, lastWriteTime ?? MemoryEntry.DefaultLastWriteTime));

    public void Add(string filePath, byte[] binaryContent, DateTime? lastWriteTime = null) =>
        Add(new MemoryBinaryFileEntry(filePath, lastWriteTime ?? MemoryEntry.DefaultLastWriteTime, binaryContent));

    public void Add(string filePath, string textContent, DateTime? lastWriteTime = null) =>
        Add(new MemoryBinaryFileEntry(filePath, lastWriteTime ?? MemoryEntry.DefaultLastWriteTime, textContent));

    public void Add(string filePath, ZipFileSystem zipFile, DateTime? lastWriteTime = null) =>
        Add(new MemoryZipFileEntry(filePath, lastWriteTime ?? MemoryEntry.DefaultLastWriteTime, zipFile));

    public void Add(string filePath, Exception exception, DateTime? lastWriteTime = null) =>
        Add(new MemoryReadFailFileEntry(filePath, lastWriteTime ?? MemoryEntry.DefaultLastWriteTime, exception));

    public void Add(MemoryEntry entry) {
        entry = entry with { Path = NormalizePath(entry.Path) };

        if (_Entries.ContainsKey(entry.Path)) {
            throw new InvalidOperationException($"Path '{entry.Path}' already exists.");
        }

        var path = GetParentPath(entry.Path);
        if (path != null) {
            VerifyParents(path, entry.LastWriteTime);
        }

        _Entries.TryAdd(entry.Path, entry);
    }

    public void AddRange(IEnumerable<MemoryEntry> entries) {
        foreach (var entry in entries) {
            Add(entry);
        }
    }

    public TEntry? FindEntry<TEntry>(string path) where TEntry : MemoryEntry =>
        _Entries.TryGetValue(NormalizePath(path), out var entry) && entry is TEntry fileEntry
            ? fileEntry
            : null;

    public TEntry GetEntry<TEntry>(string path) where TEntry : MemoryEntry =>
        FindEntry<TEntry>(path) ?? throw (
            typeof(MemoryDirectoryEntry).IsAssignableFrom(typeof(TEntry))
                ? new DirectoryNotFoundException($"Directory '{path}' not found.")
                : new FileNotFoundException($"File '{path}' not found.")
        );

    public delegate T Updater<T>(T instance);

    public void UpdateEntry<TEntry>(string path, Updater<TEntry> updater) where TEntry : MemoryEntry {
        var entry = updater(GetEntry<TEntry>(path));
        if (entry.Path != path) {
            _Entries.TryRemove(path, out _);
        }

        _Entries[entry.Path] = entry;
    }

    public void DeleteEntry<TEntry>(string path)  where TEntry : MemoryEntry {
        _Entries.TryGetValue(path, out var entry);
        if (entry is MemoryDirectoryEntry) {
            throw new InvalidOperationException($"Entry at '{path}' is directory.");
        }

        if (entry is MemoryFileEntry { Locked: true }) {
            throw new InvalidOperationException($"File at '{path}' is locked.");
        }

        _Entries.TryRemove(path, out _);
        
    }

    public IEnumerable<MemoryEntry> EnumerateEntries(string path, string searchPattern, SearchOption searchOption) {
        if (string.IsNullOrEmpty(searchPattern)) {
            throw new ArgumentNullException(nameof(searchPattern), "Search pattern cannot be empty.");
        }

        path = NormalizePath(path);

        GetEntry<MemoryDirectoryEntry>(path);

        // _Items.Keys = [@"C:\", @"C:\Foo", @"C:\Test", @"C:\Test\Dir1", @"C:\Test\Dir2", @"C:\Test\File1.txt", @"C:\Test\Dir3", @"C:\Test\Dir3\SubDir", @"C:\Test\Dir3\File2.txt" ]
        // path = @"C:\Test"

        // filter out all where Key do not start with path
        var query = _Entries.Where(o => o.Key.StartsWith(path, StringComparison.OrdinalIgnoreCase));

        // _Items.Keys = [@"C:\Test", @"C:\Test\Dir1", @"C:\Test\Dir2", @"C:\Test\File1.txt", @"C:\Test\Dir3", @"C:\Test\Dir3\SubDir", @"C:\Test\Dir3\File2.txt" ]

        // filter out 'self'
        query = query.Where(o => o.Key.Length > path.Length);

        // _Items.Keys = [ @"C:\Test\Dir1", @"C:\Test\Dir2", @"C:\Test\File1.txt", @"C:\Test\Dir3", @"C:\Test\Dir3\SubDir", @"C:\Test\Dir3\File2.txt" ]
        if (searchOption == SearchOption.TopDirectoryOnly) {
            // filter out nested entries
            query = query.Where(o => {
                // o.Key one of = [ @"C:\Test\Dir1", @"C:\Test\Dir2", @"C:\Test\File1.txt", @"C:\Test\Dir3", @"C:\Test\Dir3\SubDir", @"C:\Test\Dir3\File2.txt" ]
                var index = o.Key.IndexOfAny([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar], path.Length + 1);
                if (index == -1) {
                    // o.Key one of [ @"C:\Test\Dir1", @"C:\Test\Dir2", @"C:\Test\File1.txt", @"C:\Test\Dir3" ]
                    return true;
                }

                // o.Key one of [ @"C:\Test\Dir3\SubDir", @"C:\Test\Dir3\File2.txt" ]
                return false;
            });
        }

        var regex = ToRegex(searchPattern);

        // filter out files that do not match pattern
        query = query.Where(o => regex.IsMatch(Path.GetFileName(o.Key)));

        return query.Select(o => o.Value).OrderBy(o => o!.Path);
    }

    private static Regex ToRegex(string searchPattern) {
        var invalidPathChars = Path.GetInvalidFileNameChars();
        if (searchPattern.Any(o => (o != '?') & (o != '*') && invalidPathChars.Contains(o))) {
            throw new ArgumentException("Invalid search pattern.");
        }

        var regexPattern = searchPattern == "*.*"
            ? $"[^{Path.DirectorySeparatorChar}{Path.AltDirectorySeparatorChar}]*"
            : Regex.Escape(searchPattern).Replace("\\*", ".*").Replace("\\?", ".");

        return new("^" + regexPattern + "$", RegexOptions.IgnoreCase);
    }

    internal abstract string NormalizePath(string path);

    protected abstract string? GetParentPath(string path);

    public void LockFile(string path) => SetLock(path, true);

    public void UnlockFile(string path) => SetLock(path, false);

    private void SetLock(string path, bool locked) {
        path = NormalizePath(path);
        UpdateEntry<MemoryFileEntry>(path, o => o with { Locked = locked });
    }

    protected void VerifyParents(string folderPath, DateTime lastWriteTime) {
        var paths = new Stack<string>();
        
        var current = folderPath;
        while (current is { Length: > 0 }) {
            paths.Push(current);
            current = Path.GetDirectoryName(current);
        }

        while (paths.Any()) {
            var directoryPath = paths.Pop()!;
            if (_Entries.TryGetValue(directoryPath, out var directoryEntry)) {
                if (directoryEntry is not MemoryDirectoryEntry) {
                    throw new InvalidOperationException($"Path '{directoryPath}' is a file, not a directory.");
                }

                _Entries[directoryPath] = directoryEntry with { LastWriteTime = lastWriteTime };
            } else {
                _Entries.TryAdd(directoryPath, new MemoryDirectoryEntry(directoryPath, lastWriteTime));
            }
        }
    }

    public IEnumerator<MemoryEntry> GetEnumerator() => _Entries.Values.OrderBy(o => o.Path).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
