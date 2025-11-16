using System;
using System.IO;
using System.Linq;
using MockFileSystem.Wrappers;
using NSubstitute;
using Shouldly;

namespace MockFileSystem.Tests.Wrappers;

public class TestsMemoryDirectoryInfo
{
    [Fact]
    public void EnumerateFiles_TopDirectoryOnly() {
        // Arrange
        var fileSystem = new MemoryFileSystem {
            @"C:\Path\Foo",
            { @"C:\Path\Bar.txt", "File" },
            { @"C:\Path\Baz\Baz.txt", "File" }
        };
        var sut = new MemoryDirectoryInfo(fileSystem, @"C:\Path");

        // Act
        var files = sut.EnumerateFiles("*").ToArray();

        // Assert
        files.Length.ShouldBe(1);
        files[0].FullName.ShouldBe(@"C:\Path\Bar.txt");
    }
    
    [Fact]
    public void EnumerateFiles_AllDirectories() {
        // Arrange
        var fileSystem = new MemoryFileSystem {
            { @"C:\Path\Baz\Baz.txt", "File" },
            { @"C:\Path\Bar.txt", "File" },
            @"C:\Path\Foo",
        };
        var sut = new MemoryDirectoryInfo(fileSystem, @"C:\Path");

        // Act
        var files = sut.EnumerateFiles("*", SearchOption.AllDirectories).ToArray();

        // Assert
        files.Length.ShouldBe(2);
        files[0].FullName.ShouldBe(@"C:\Path\Bar.txt");
        files[1].FullName.ShouldBe(@"C:\Path\Baz\Baz.txt");
    }
    
    [Fact]
    public void EnumerateFiles_ThrowEmptyPattern() {
        // Arrange
        var fileSystem = new MemoryFileSystem {
            { @"C:\Path\Baz\Baz.txt", "File" },
            { @"C:\Path\Bar.txt", "File" },
            @"C:\Path\Foo",
        };
        var sut = new MemoryDirectoryInfo(fileSystem, @"C:\Path");

        // Act
        var act = () => sut.EnumerateFiles("", SearchOption.AllDirectories).ToArray();

        // Assert
        act.ShouldThrow<ArgumentNullException>().Message.ShouldStartWith("Search pattern cannot be empty.");

    }
    
    [Fact]
    public void Mock_CallBaseAndRecordCallsCorrectly() {
        // Arrange
        var fileSystem = new MemoryFileSystem {
            @"C:\Path\Foo",
            { @"C:\Path\Bar.txt", "File" },
            { @"C:\Path\Baz\Baz.txt", "File" }
        };
        var sut = new MemoryDirectoryInfo(fileSystem, @"C:\Path").Mock();

        // Act
        var files = sut.EnumerateFiles("*").ToArray();

        // Assert
        files.Length.ShouldBe(1);
        sut.Received().EnumerateFiles("*");
    }
}
