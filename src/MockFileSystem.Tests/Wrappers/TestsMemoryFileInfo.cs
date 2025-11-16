using System;
using System.IO;
using System.Linq;
using MockFileSystem.Entries;
using MockFileSystem.Wrappers;
using NSubstitute;
using Shouldly;

namespace MockFileSystem.Tests.Wrappers;

public class TestsMemoryFileInfo
{
    [Fact]
    public void LastWriteTime() {
        // Arrange
        var lastWriteTime = DateTime.Now;
        var fileSystem = new MemoryFileSystem {
            { @"C:\Path\File.txt", "File", lastWriteTime }
        };
        var sut = new MemoryFileInfo(fileSystem, @"C:\Path\File.txt");

        // Act
        var actual = sut.LastWriteTime;

        // Assert
        actual.ShouldBe(lastWriteTime);
    }

    [Fact]
    public void LastWriteTime_WhenNotFound() {
        // Arrange
        var fileSystem = new MemoryFileSystem();
        var sut        = new MemoryFileInfo(fileSystem, @"C:\Path\File.txt");

        // Act & Assert
        Should.Throw<FileNotFoundException>(() => sut.LastWriteTime)
            .Message.ShouldBe(@"File 'C:\Path\File.txt' not found.");
    }

    [Fact]
    public void FullName() {
        // Arrange
        var fileSystem = new MemoryFileSystem {
            { @"C:\Path\File.txt", "File" }
        };
        var sut = new MemoryFileInfo(fileSystem, @"C:\Path\File.txt");

        // Act
        var actual = sut.FullName;

        // Assert
        actual.ShouldBe(@"C:\Path\File.txt");
    }

    [Fact]
    public void MoveTo() {
        // Arrange
        var fileSystem = new MemoryFileSystem {
            { @"C:\Path\File.txt", "File" }
        };
        var sut = new MemoryFileInfo(fileSystem, @"C:\Path\File.txt");

        // Act
        sut.MoveTo(@"C:\Path\Target.txt");

        // Assert
        sut.FullName.ShouldBe(@"C:\Path\Target.txt");
        var entries = fileSystem.ToArray();
        entries.Length.ShouldBe(3);
        entries[0].ShouldBeOfType<MemoryDirectoryEntry>().Path.ShouldBe(@"C:\");
        entries[1].ShouldBeOfType<MemoryDirectoryEntry>().Path.ShouldBe(@"C:\Path");
        entries[2].ShouldBeOfType<MemoryBinaryFileEntry>().Path.ShouldBe(@"C:\Path\Target.txt");
    }
    
    [Fact]
    public void Mock_RecordCallsCorrectly() {
        // Arrange
        var lastWriteTime = DateTime.Now;
        var fileSystem = new MemoryFileSystem {
            { @"C:\Path\File.txt", "File", lastWriteTime }
        };
        var sut = new MemoryFileInfo(fileSystem, @"C:\Path\File.txt").Mock();

        // Act
        var actual   = sut.LastWriteTime;
        var fullName = sut.FullName;
        sut.MoveTo(@"C:\Path\Target.txt");

        // Assert
        actual.ShouldBe(lastWriteTime);
        fullName.ShouldBe(@"C:\Path\File.txt");
        fileSystem.GetEntry<MemoryBinaryFileEntry>(@"C:\Path\Target.txt").ShouldNotBeNull();

        _ = sut.Received().LastWriteTime;
        _ = sut.Received().FullName;
        sut.Received().MoveTo(@"C:\Path\Target.txt");
    }
}
