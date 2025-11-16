using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using MockFileSystem.Entries;

namespace MockFileSystem.Utility;

[ExcludeFromCodeCoverage]
[DebuggerTypeProxy(typeof(EntryDictionaryProxy))]
public sealed class EntryDictionary() : ConcurrentDictionary<string, MemoryEntry>(StringComparer.OrdinalIgnoreCase);