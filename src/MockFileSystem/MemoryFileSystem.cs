using System.Diagnostics.CodeAnalysis;
using System.IO;
using JetBrains.Annotations;
using MockFileSystem.Entries;

namespace MockFileSystem;

[PublicAPI]
public sealed class MemoryFileSystem : BaseFileSystem
{
    public MemoryFileSystem(string? currentDirectory = null) => CurrentDirectory = currentDirectory ?? "C:\\";

    private string _CurrentDirectory = null!;

    public string CurrentDirectory {
        get => _CurrentDirectory;
        [ExcludeFromCodeCoverage]
        set {
            var normalized = NormalizePath(value);
            VerifyParents(normalized, MemoryEntry.DefaultLastWriteTime);
            _CurrentDirectory = normalized;
        }
    }

    internal override string NormalizePath(string path) {
        // Resolve relative paths against _currentDirectory
        if (!Path.IsPathRooted(path)) {
            path = Path.Combine(CurrentDirectory, path);
        }

        path = Path.GetFullPath(path);
        if (path.Length > 3) { // Trim trailing slash for non-root paths
            path = path.TrimEnd('\\');
        }

        return path;
    }

    protected override string? GetParentPath(string path) => Path.GetDirectoryName(path);
}