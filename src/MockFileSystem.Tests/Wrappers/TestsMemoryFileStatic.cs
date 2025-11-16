using System;
using System.IO;
using System.Linq;
using MockFileSystem.Entries;
using MockFileSystem.Utility;
using MockFileSystem.Wrappers;
using NSubstitute;
using Shouldly;

namespace MockFileSystem.Tests.Wrappers;

public class TestsMemoryFileStatic
{
    [Theory]
    [InlineData(null!)]
    [InlineData("Folder")]
    [InlineData("File")]
    public void Exists(string? type) {
        // Arrange
        var fileSystem = new MemoryFileSystem();
        switch (type) {
            case "Folder": fileSystem.Add(@"c:\path"); break;
            case "File":   fileSystem.Add(@"c:\path", "Content"); break;
        }

        var sut = new MemoryFileStatic(fileSystem);

        // Act
        var actual = sut.Exists(@"C:\path");

        // Assert
        actual.ShouldBe(type == "File");
    }

    [Fact]
    public void ReadAllText_WhenValid() {
        // Arrange
        var fileSystem = new MemoryFileSystem {
            { @"C:\Path", "Content" }
        };

        var sut = new MemoryFileStatic(fileSystem);

        // Act
        var actual = sut.ReadAllText(@"C:\path");

        // Assert
        actual.ShouldBe("Content");
    }

    [Fact]
    public void ReadAllText_ThrowsWhenException() {
        // Arrange
        var exception = new Exception();
        var fileSystem = new MemoryFileSystem {
            { @"C:\Path", exception }
        };
        var sut = new MemoryFileStatic(fileSystem);

        // Act & Assert
        Should.Throw<Exception>(() => sut.ReadAllText(@"C:\path")).ShouldBe(exception);
    }
    
    [Fact]
    public void ReadAllText_ThrowsWhenZip() {
        // Arrange
        var fileSystem = new MemoryFileSystem {
            { @"C:\Path", new ZipFileSystem() }
        };
        var sut = new MemoryFileStatic(fileSystem);

        // Act & Assert
        Should.Throw<NotSupportedException>(() => sut.ReadAllText(@"C:\path")).Message.ShouldBe($"Not supported file type: {nameof(MemoryZipFileEntry)}");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ReadAllText_ThrowsWhenNotFoundOrDirectory(bool isDirectory) {
        // Arrange
        var fileSystem = new MemoryFileSystem();
        if (isDirectory) {
            fileSystem.Add(@"C:\Path");
        }

        var sut = new MemoryFileStatic(fileSystem);

        // Act & Assert
        Should.Throw<FileNotFoundException>(() => sut.ReadAllText(@"c:\path"))
            .Message.ShouldBe(@"File 'c:\path' not found.");
    }

    [Fact]
    public void WriteAllText_OverrideExisting() {
        // Arrange
        var fileSystem = new MemoryFileSystem {
            { @"C:\Path", "Content" }
        };

        var sut = new MemoryFileStatic(fileSystem);

        // Act
        sut.WriteAllText(@"C:\Path", "content");

        // Assert
        var entries = fileSystem.ToArray();
        entries.ShouldContain(o => o.Path == @"C:\Path");
        var file = entries.First(o => o.Path == @"C:\Path");
        file.ShouldBeOfType<MemoryBinaryFileEntry>().StringContent.ShouldBe("content");

    }
    
    [Fact]
    public void WriteAllText_CreateNew() {
        // Arrange
        var fileSystem = new MemoryFileSystem();
        var sut        = new MemoryFileStatic(fileSystem);

        // Act
        sut.WriteAllText(@"C:\Path", "content");

        // Assert
        var entries = fileSystem.ToArray();
        entries.ShouldContain(o => o.Path == @"C:\Path");
        var file = entries.First(o => o.Path == @"C:\Path");
        file.ShouldBeOfType<MemoryBinaryFileEntry>().StringContent.ShouldBe("content");

    }
    
    [Fact]
    public void GetLastWriteTime_WhenValid() {
        // Arrange
        var fileSystem = new MemoryFileSystem {
            { @"c:\path", "Content" }
        };
        var sut = new MemoryFileStatic(fileSystem);

        // Act
        var actual = sut.GetLastWriteTime(@"C:\path");

        // Assert
        actual.ShouldBe(MemoryEntry.DefaultLastWriteTime);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void GetLastWriteTime_ThrowsWhenNotFoundOrDirectory(bool isDirectory) {
        // Arrange
        var fileSystem = new MemoryFileSystem();
        if (isDirectory) {
            fileSystem.Add(@"c:\path");
        }

        var sut = new MemoryFileStatic(fileSystem);

        // Act & Assert
        Should.Throw<FileNotFoundException>(() => sut.GetLastWriteTime(@"c:\path"))
            .Message.ShouldBe(@"File 'c:\path' not found.");
    }

    [Fact]
    public void Delete_WhenValid() {
        // Arrange
        var fileSystem = new MemoryFileSystem {
            { @"c:\path", "Content" }
        };
        var sut = new MemoryFileStatic(fileSystem);

        // Act
        sut.Delete(@"C:\path");

        // Assert
        fileSystem.ToArray().Length.ShouldBe(1);
    }

    [Fact]
    public void Delete_DoNothingWhenNotFound() {
        // Arrange
        var fileSystem = new MemoryFileSystem {
            { @"C:\foo", "Content" }
        };
        var sut = new MemoryFileStatic(fileSystem);

        // Act
        sut.Delete(@"C:\path");

        // Assert
        fileSystem.ToArray().Length.ShouldBe(2);
    }

    [Fact]
    public void Delete_ThrowsWhenDirectory() {
        // Arrange
        var fileSystem = new MemoryFileSystem {
            @"c:\path"
        };
        var sut = new MemoryFileStatic(fileSystem);

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => sut.Delete(@"c:\path"))
            .Message.ShouldBe(@"Entry at 'c:\path' is directory.");
    }

    [Fact]
    public void Delete_ThrowsWhenLocked() {
        // Arrange
        var fileSystem = new MemoryFileSystem {
            { @"c:\path", "Content" }
        };
        fileSystem.LockFile(@"c:\path");
        var sut = new MemoryFileStatic(fileSystem);

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => sut.Delete(@"c:\path"))
            .Message.ShouldBe(@"File at 'c:\path' is locked.");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Move_WhenSourceNotFoundOrDirectory(bool isDirectory) {
        // Arrange
        var fileSystem = new MemoryFileSystem();
        if (isDirectory) {
            fileSystem.Add(@"c:\path");
        }

        var sut = new MemoryFileStatic(fileSystem);

        // Act & Assert
        Should.Throw<FileNotFoundException>(() => sut.Move(@"c:\path", @"c:\target"))
            .Message.ShouldBe(@"File 'c:\path' not found.");
    }

    [Fact]
    public void Move_WhenDestinationExists() {
        // Arrange
        var fileSystem = new MemoryFileSystem();
        fileSystem.Add(@"c:\path", "Source");
        fileSystem.Add(@"c:\target", "target");
        var sut = new MemoryFileStatic(fileSystem);

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => sut.Move(@"C:\path", @"C:\target"))
            .Message.ShouldBe(@"Destination 'C:\target' exists.");
    }

    [Fact]
    public void Move_WhenSourceLocked() {
        // Arrange
        var fileSystem = new MemoryFileSystem {
            { @"C:\path", "Source" }
        };
        fileSystem.LockFile(@"C:\path");
        var sut = new MemoryFileStatic(fileSystem);

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => sut.Move(@"C:\path", @"C:\target"))
            .Message.ShouldBe(@"File at 'C:\path' is locked.");
    }

    [Fact]
    public void Move_WhenValid() {
        // Arrange
        var fileSystem = new MemoryFileSystem {
            { @"c:\path", [1, 2, 3] }
        };
        var sut = new MemoryFileStatic(fileSystem);

        // Act
        sut.Move(@"C:\path", @"C:\target");

        // Assert
        var entries = fileSystem.ToArray();
        entries.ShouldContain(o => o.Path == @"C:\target" && o is MemoryBinaryFileEntry);
    }

    [Fact]
    public void Create_WhenAddThrows() {
        // Arrange
        var fileSystem = new MemoryFileSystem {
            { @"c:\path", [1, 2, 3] }
        };
        var sut = new MemoryFileStatic(fileSystem);

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => sut.Create(@"C:\path"));
    }

    [Fact]
    public void Create_WhenSucceed() {
        // Arrange
        var fileSystem = new MemoryFileSystem();
        var sut        = new MemoryFileStatic(fileSystem);

        // Act
        var actual = sut.Create(@"C:\path");

        // Assert
        actual.ShouldBeOfType<MemoryFileStream>();
        var entries = fileSystem.ToArray();

        entries.Length.ShouldBe(2);
        entries[0].Path.ShouldBe(@"C:\");
        entries[1].Path.ShouldBe(@"C:\path");
        entries[1].ShouldBeOfType<MemoryBinaryFileEntry>().Content.ShouldBeEmpty();

        byte[] buffer = [1, 2, 3];
        actual.Write(buffer, 0, 3);
        actual.Dispose();
        entries = fileSystem.ToArray();
        entries[1].ShouldBeOfType<MemoryBinaryFileEntry>().Content.ShouldBeEquivalentTo(buffer);
    }
    
    [Fact]
    public void Mock_RecordCallsCorrectly() {
        // Arrange
        var fileSystem = new MemoryFileSystem(@"C:\Path") {
            @"C:\Path\Foo",
            { @"C:\Path\Bar.txt", "File" },
            { @"C:\Path\Baz\Baz.txt", "File" }
        };
        var sut = new MemoryFileStatic(fileSystem).Mock();

        // Act
        var exists = sut.Exists(@"C:\Path\Bar.txt");
        var readAllText = sut.ReadAllText(@"C:\Path\Bar.txt");
        sut.WriteAllText(@"C:\Path\Fizz.txt", "FILE");
        var lastWriteTime = sut.GetLastWriteTime(@"C:\Path\Bar.txt");
        sut.Delete(@"C:\Path\Baz\Baz.txt");
        sut.Move(@"C:\Path\Bar.txt", @"C:\Path\Baz.txt");
        var create = sut.Create(@"C:\Path\Foo.txt");
        
        // Assert
        exists.ShouldBeTrue();
        readAllText.ShouldBe("File");
        fileSystem.FindEntry<MemoryBinaryFileEntry>(@"C:\Path\Fizz.txt").ShouldNotBeNull().StringContent.ShouldBe("FILE");
        lastWriteTime.ShouldBe(MemoryEntry.DefaultLastWriteTime);
        fileSystem.FindEntry<MemoryBinaryFileEntry>(@"C:\Path\Baz\Baz.txt").ShouldBeNull();
        fileSystem.FindEntry<MemoryBinaryFileEntry>(@"C:\Path\Baz.txt").ShouldNotBeNull().StringContent.ShouldBe("File");
        create.ShouldNotBeNull();
        fileSystem.FindEntry<MemoryBinaryFileEntry>(@"C:\Path\Foo.txt").ShouldNotBeNull().Content.ShouldBeEquivalentTo(Array.Empty<byte>());
        create.Write([1, 2, 3], 0, 3);
        create.Dispose();
        fileSystem.FindEntry<MemoryBinaryFileEntry>(@"C:\Path\Foo.txt").ShouldNotBeNull().Content.ShouldBeEquivalentTo(new byte[] { 1, 2, 3 });

        Received.InOrder(() => {
            sut.Exists(@"C:\Path\Bar.txt");
            sut.ReadAllText(@"C:\Path\Bar.txt");
            sut.WriteAllText(@"C:\Path\Fizz.txt", "FILE");
            sut.GetLastWriteTime(@"C:\Path\Bar.txt");
            sut.Delete(@"C:\Path\Baz\Baz.txt");
            sut.Move(@"C:\Path\Bar.txt", @"C:\Path\Baz.txt");
            sut.Create(@"C:\Path\Foo.txt");
        });
    }
}
