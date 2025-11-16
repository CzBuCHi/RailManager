using System;
using System.Linq;
using JetBrains.Annotations;

namespace MockFileSystem;

[PublicAPI]
public sealed class ZipFileSystem : BaseFileSystem
{
    internal override string NormalizePath(string path) {
        if (path.Contains(':')) {
            throw new ArgumentException("Zip file do not support absolute paths.");
        }

        path = path.Replace('\\', '/');
        if (path.StartsWith("/")) {
            path = path.Substring(1); // Remove leading "/"
        }

        return path;
    }

    protected override string? GetParentPath(string path) {
        var index = path.LastIndexOf('/');
        return index == -1 ? null : path.Substring(0, index);
    }
    

}