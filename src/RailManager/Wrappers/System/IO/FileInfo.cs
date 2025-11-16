using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace RailManager.Wrappers.System.IO;

/// <summary>
///     Provides a mockable abstraction over <see cref="FileInfo" /> for use in mod management systems.
///     This interface exposes essential directory metadata and enumeration capabilities,
///     allowing full test control over directory structure, existence, and contents without real file system access.
/// </summary>
public interface IFileInfo
{
    /// <inheritdoc cref="FileInfo.LastWriteTime" />
    DateTime LastWriteTime { get; }

    /// <inheritdoc cref="FileInfo.FullName" />
    string FullName { get; }

    /// <inheritdoc cref="FileInfo.MoveTo(string)" />
    void MoveTo(string destFileName);
}

[ExcludeFromCodeCoverage]
public sealed class FileInfoWrapper(FileInfo fileInfo) : IFileInfo
{
    public DateTime LastWriteTime => fileInfo.LastWriteTime;

    public string FullName => fileInfo.FullName;

    public void MoveTo(string destFileName) => fileInfo.MoveTo(destFileName);
}
