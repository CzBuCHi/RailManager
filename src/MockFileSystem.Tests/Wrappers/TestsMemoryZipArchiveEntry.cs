using System;
using System.IO;
using MockFileSystem.Entries;
using MockFileSystem.Wrappers;
using Shouldly;

namespace MockFileSystem.Tests.Wrappers;

public class TestsMemoryZipArchiveEntry
{
    [Fact]
    public void FullName() {
        // Arrange
        var entry = new MemoryBinaryFileEntry(@"C:\Path\File.txt", MemoryEntry.DefaultLastWriteTime, [1, 2, 3]);
        var sut   = new MemoryZipArchiveEntry(entry);

        // Act
        var actual = sut.FullName;

        // Assert
        actual.ShouldBe(@"C:\Path\File.txt");
    }

    [Fact]
    public void Name() {
        // Arrange
        var entry = new MemoryBinaryFileEntry(@"C:\Path\File.txt", MemoryEntry.DefaultLastWriteTime, [1, 2, 3]);
        var sut   = new MemoryZipArchiveEntry(entry);

        // Act
        var actual = sut.Name;

        // Assert
        actual.ShouldBe("File.txt");
    }

    [Fact]
    public void Open() {
        // Arrange
        byte[] content = [1, 2, 3];
        var    entry   = new MemoryBinaryFileEntry(@"C:\Path\File.txt", MemoryEntry.DefaultLastWriteTime, [1, 2, 3]);
        var    sut     = new MemoryZipArchiveEntry(entry);

        // Act
        var actual = sut.Open();

        // Assert
        var stream = actual.ShouldBeOfType<MemoryStream>();
        stream.ToArray().ShouldBeEquivalentTo(content);
    }
    
    [Fact]
    public void Open_ThrowsForUnsupported() {
        // Arrange
        var    entry   = new MemoryZipFileEntry(@"C:\Path\File.txt", MemoryEntry.DefaultLastWriteTime, new());
        var    sut     = new MemoryZipArchiveEntry(entry);

        // Act
        var act = () => sut.Open();

        // Assert
        act.ShouldThrow<NotSupportedException>().Message.ShouldBe($"Not supported file type: MemoryZipFileEntry");
    }
}
