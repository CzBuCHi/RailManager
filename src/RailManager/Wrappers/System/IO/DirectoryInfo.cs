using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using JetBrains.Annotations;

namespace RailManager.Wrappers.System.IO;

/// <summary>
///     Provides a mockable abstraction over <see cref="DirectoryInfo" /> for use in mod management systems.
///     This interface exposes essential directory metadata and enumeration capabilities,
///     allowing full test control over directory structure, existence, and contents without real file system access.
/// </summary>
[PublicAPI]
public interface IDirectoryInfo
{
    /// <inheritdoc cref="DirectoryInfo.EnumerateFiles(string, SearchOption)" />
    IEnumerable<IFileInfo> EnumerateFiles(
        string searchPattern,
        SearchOption searchOption = SearchOption.TopDirectoryOnly
    );
}

[ExcludeFromCodeCoverage]
public sealed class DirectoryInfoWrapper(DirectoryInfo directoryInfo) : IDirectoryInfo
{
    /// <inheritdoc />
    public IEnumerable<IFileInfo> EnumerateFiles(string searchPattern, SearchOption searchOption) =>
        directoryInfo.EnumerateFiles(searchPattern, searchOption).Select(o => new FileInfoWrapper(o));
}
