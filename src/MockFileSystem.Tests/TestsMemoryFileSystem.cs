using System;
using System.Collections.Generic;
using System.Linq;
using MockFileSystem.Entries;
using Shouldly;

namespace MockFileSystem.Tests;

public class TestsMemoryFileSystem
{
    [Fact]
    public void ReturnsCorrectInstanceOfDirectoryInfo() {
        // Arrange
        var sut = new MemoryFileSystem {
            { @"C:\Path\File.txt", "File" }
        };

        // Act
        var directoryInfo = sut.DirectoryInfo(@"C:\Path");

        // Assert
        var files = directoryInfo.EnumerateFiles("*").ToArray();
        files.Length.ShouldBe(1);
        files[0].FullName.ShouldBe(@"C:\Path\File.txt");
    }

    [Fact]
    public void Throws_WhenTryingToAddToInvalidPath() {
        // Arrange
        var sut = new MemoryFileSystem {
            { @"C:\Path\File.txt", "File" }
        };
        // Act
        var act = () => sut.Add(@"C:\Path\File.txt\Dir");

        // Assert
        act.ShouldThrow<InvalidOperationException>().Message.ShouldBe(@"Path 'C:\Path\File.txt' is a file, not a directory.");
    }

    [Fact]
    public void Throws_InvalidSearchPattern() {
        // Arrange
        var sut = new MemoryFileSystem {
            { @"C:\Path\File.txt", "File" }
        };
        var directoryInfo = sut.DirectoryInfo(@"C:\Path");

        // Act
        var act = () => directoryInfo.EnumerateFiles("a\\b").ToArray();

        // Assert
        act.ShouldThrow<ArgumentException>().Message.ShouldBe("Invalid search pattern.");
    }
    
    private static readonly MemoryEntry[] _EnumerateSpecificWildcardPatternsMatchesCorrectlyEntries = [
        // @formatter:off
        new MemoryBinaryFileEntry(@"c:\test\__.__",  MemoryEntry.DefaultLastWriteTime, [0 ] ),
        new MemoryBinaryFileEntry(@"c:\test\-.__",   MemoryEntry.DefaultLastWriteTime, [1 ] ),
        new MemoryBinaryFileEntry(@"c:\test\__.-",   MemoryEntry.DefaultLastWriteTime, [2 ] ),
        new MemoryBinaryFileEntry(@"c:\test\-.-",    MemoryEntry.DefaultLastWriteTime, [3 ] ),
                                                     
        new MemoryBinaryFileEntry(@"c:\test\a__.__", MemoryEntry.DefaultLastWriteTime, [4 ] ),
        new MemoryBinaryFileEntry(@"c:\test\a-.__",  MemoryEntry.DefaultLastWriteTime, [5 ] ),
        new MemoryBinaryFileEntry(@"c:\test\a__.-",  MemoryEntry.DefaultLastWriteTime, [6 ] ),
        new MemoryBinaryFileEntry(@"c:\test\a-.-",   MemoryEntry.DefaultLastWriteTime, [7 ] ),
                                                     
        new MemoryBinaryFileEntry(@"c:\test\__b.__", MemoryEntry.DefaultLastWriteTime, [8 ] ),
        new MemoryBinaryFileEntry(@"c:\test\-b.__",  MemoryEntry.DefaultLastWriteTime, [9 ] ),
        new MemoryBinaryFileEntry(@"c:\test\__b.-",  MemoryEntry.DefaultLastWriteTime, [10] ),
        new MemoryBinaryFileEntry(@"c:\test\-b.-",   MemoryEntry.DefaultLastWriteTime, [11] ),
                                                     
        new MemoryBinaryFileEntry(@"c:\test\__.c__", MemoryEntry.DefaultLastWriteTime, [12] ),
        new MemoryBinaryFileEntry(@"c:\test\-.c__",  MemoryEntry.DefaultLastWriteTime, [13] ),
        new MemoryBinaryFileEntry(@"c:\test\__.c-",  MemoryEntry.DefaultLastWriteTime, [14] ),
        new MemoryBinaryFileEntry(@"c:\test\-.c-",   MemoryEntry.DefaultLastWriteTime, [15] ),
                                                     
        new MemoryBinaryFileEntry(@"c:\test\__.__d", MemoryEntry.DefaultLastWriteTime, [16] ),
        new MemoryBinaryFileEntry(@"c:\test\-.__d",  MemoryEntry.DefaultLastWriteTime, [17] ),
        new MemoryBinaryFileEntry(@"c:\test\__.-d",  MemoryEntry.DefaultLastWriteTime, [18] ),
        new MemoryBinaryFileEntry(@"c:\test\-.-d",   MemoryEntry.DefaultLastWriteTime, [19] )
        // @formatter:on
    ];
    
    public static IEnumerable<object?[]> Enumerate_SpecificWildcardPatterns_MatchesCorrectlyData() {
        return Enumerate().Select(o => new object?[] {
            o.searchPattern,
            o.entries!.Select(p => _EnumerateSpecificWildcardPatternsMatchesCorrectlyEntries[p]).ToArray()
        });

        static IEnumerable<(string searchPattern, int[] entries)> Enumerate() {
            // @formatter:off
            yield return ("*.*",  [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19]);
            yield return ("?.*",  [   1,    3,                               13,     15,     17,     19]);
            yield return ("*.?",  [      2, 3,       6, 7,       10, 11                                ]);
            yield return ("?.?",  [         3                                                          ]);
            yield return ("a*.*", [            4, 5, 6, 7                                              ]);
            yield return ("a?.*", [               5,    7                                              ]);
            yield return ("a*.?", [                  6, 7                                              ]);
            yield return ("a?.?", [                     7                                              ]);
            yield return ("*b.*", [                        8, 9, 10, 11                                ]);
            yield return ("?b.*", [                           9,     11                                ]);
            yield return ("*b.?", [                              10, 11                                ]);
            yield return ("?b.?", [                                  11                                ]);
            yield return ("*.c*", [                                      12, 13, 14, 15                ]);
            yield return ("?.c*", [                                          13,     15                ]);
            yield return ("*.c?", [                                              14, 15                ]);
            yield return ("?.c?", [                                                  15                ]);
            yield return ("*.*d", [                                                      16, 17, 18, 19]);
            yield return ("?.*d", [                                                          17,     19]);
            yield return ("*.?d", [                                                              18, 19]);
            yield return ("?.?d", [                                                                  19]);
            // @formatter:on
        }
    }

    [Theory]
    [MemberData(nameof(Enumerate_SpecificWildcardPatterns_MatchesCorrectlyData))]
    public void Enumerate_SpecificWildcardPatterns_MatchesCorrectly(string searchPattern, MemoryEntry[] entries) {
        // Arrange
        var sut = new MemoryFileSystem();
        sut.AddRange(_EnumerateSpecificWildcardPatternsMatchesCorrectlyEntries);
        
        // Act
        var result = sut.DirectoryInfo(@"C:\Test").EnumerateFiles(searchPattern).OrderBy(o => o.FullName).Select(o => o.FullName).ToArray();

        // Assert
        result.ShouldBeEquivalentTo(entries.OrderBy(o => o.Path).Select(o => o.Path).ToArray(), "pattern is " + searchPattern);
    }

    [Fact]
    public void Enumerate_Anything_ReturnsAllInDirectory() {
        // Arrange
        var sut = new MemoryFileSystem {
            { @"C:\Path\Foo.txt", "File" },
            { @"C:\Path\Bar.txt", "File" },
        };
        var directoryInfo = sut.DirectoryInfo(@"C:\Path");

        // Act
        var files = directoryInfo.EnumerateFiles("*.*").ToArray();

        // Assert
        files.Length.ShouldBe(2);
        files[0].FullName.ShouldBe(@"C:\Path\Bar.txt");
        files[1].FullName.ShouldBe(@"C:\Path\Foo.txt");
    }

    [Fact]
    public void FileLocking() {
        // Arrange
        const string path   = @"C:\Path\File.txt";
        const string target = @"C:\Path\Target.txt";
        var sut = new MemoryFileSystem {
            { path, "File" }
        };

        // Act & Assert
        sut.LockFile(path);
        Should.Throw<InvalidOperationException>(() => sut.File.Move(path, target)).Message.ShouldBe($"File at '{path}' is locked.");
        sut.UnlockFile(path);
        sut.File.Move(path, target);
    }

    [Fact]
    public void NormalizeRelativePathToCurrent() {
        // Arrange
        var sut = new MemoryFileSystem(@"C:\Current") {
            @"Nested\Folder"
        };

        // Act
        var relative = sut.Directory.Exists(@"Nested\Folder");
        var absolute = sut.Directory.Exists(@"C:\Current\Nested\Folder");

        // Assert
        relative.ShouldBeTrue();
        absolute.ShouldBeTrue();
    }
    
    [Fact]
    public void SetsLastWriteTime() {
        // Arrange
        var date = DateTime.Now;
        var sut = new MemoryFileSystem {
            { @"C:\Folder", date },
            { @"C:\Text", "content", date },
            { @"C:\Binary", [1,2], date },
            { @"C:\Zip", new ZipFileSystem(), date },
            { @"C:\Exception", new Exception(), date },
        };

        // Act
        var text      = sut.File.GetLastWriteTime(@"C:\Text");
        var binary    = sut.File.GetLastWriteTime(@"C:\Binary");
        var zip       = sut.File.GetLastWriteTime(@"C:\Zip");
        var exception = sut.File.GetLastWriteTime(@"C:\Exception");

        // Assert
        sut.GetEntry<MemoryDirectoryEntry>(@"C:\Folder").LastWriteTime.ShouldBe(date);
        text.ShouldBe(date);
        binary.ShouldBe(date);
        zip.ShouldBe(date);
        exception.ShouldBe(date);
    }
    
    [Fact]
    public void Throws_WhenAddedTwice() {
        // Arrange
        var sut = new MemoryFileSystem {
            @"C:\Folder",
        };

        // Act
        var act = () => sut.Add(@"C:\Folder");

        // Assert
        act.ShouldThrow<InvalidOperationException>().Message.ShouldBe(@"Path 'C:\Folder' already exists.");
    }
    
    [Fact]
    public void DeleteEntry_DoNothingWHenNotExists() {
        // Arrange
        var sut = new MemoryFileSystem();

        // Act
        sut.DeleteEntry<MemoryDirectoryEntry>(@"C:\Folder");

        // Assert
        sut.FindEntry<MemoryEntry>(@"C:\Folder").ShouldBeNull();
    }

    [Fact]
    public void CurrentDirectory_AddParents() {
        // Arrange
#pragma warning disable IDE0017
        var sut = new MemoryFileSystem();
#pragma warning restore IDE0017
        
        // Act
        sut.CurrentDirectory = @"C:\Path\Nested";
        
        // Assert
        var entries = sut.ToArray();
        entries.Length.ShouldBe(3);
        entries[0].Path.ShouldBe(@"C:\");
        entries[1].Path.ShouldBe(@"C:\Path");
        entries[2].Path.ShouldBe(@"C:\Path\Nested");
    }

    [Fact]
    public void NormalizePathCorrectly() {
        // Arrange
        var sut = new MemoryFileSystem();

        // Act
        sut.Add("D:");
        sut.Add(@"E:\Path\");
        
        // Assert
        var entries = sut.ToArray();
        entries.Length.ShouldBe(4);
        entries[0].Path.ShouldBe("C:\\");
        entries[1].Path.ShouldBe("D:\\");
        entries[2].Path.ShouldBe("E:\\");
        entries[3].Path.ShouldBe("E:\\Path");
    }
}
