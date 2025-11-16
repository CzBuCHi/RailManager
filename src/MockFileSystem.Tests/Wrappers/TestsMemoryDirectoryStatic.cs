using System;
using System.IO;
using System.Linq;
using MockFileSystem.Wrappers;
using NSubstitute;
using Shouldly;

namespace MockFileSystem.Tests.Wrappers;

public class TestsMemoryDirectoryStatic
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

        var sut = new MemoryDirectoryStatic(fileSystem);

        // Act
        var actual = sut.Exists(@"C:\path");

        // Assert
        actual.ShouldBe(type == "Folder");
    }

    [Fact]
    public void EnumerateDirectories() {
        // Arrange
        var fileSystem = new MemoryFileSystem {
            @"C:\Path\Folder",
            { @"C:\Path\File.txt", "File" }
        };
        var sut = new MemoryDirectoryStatic(fileSystem);

        // Act
        var actual = sut.EnumerateDirectories(@"C:\\Path").ToArray();

        // Assert
        actual.ShouldBeEquivalentTo(new[] { @"C:\Path\Folder" });
    }

    [Fact]
    public void EnumerateDirectories_ThrowsWhenNotFound() {
        // Arrange
        var fileSystem = new MemoryFileSystem();
        var sut        = new MemoryDirectoryStatic(fileSystem);

        // Act
        var act = () => sut.EnumerateDirectories(@"C:\Path").ToArray();

        // Assert
        act.ShouldThrow<DirectoryNotFoundException>().Message.ShouldBe(@"Directory 'C:\Path' not found.");
    }

    [Fact]
    public void GetCurrentDirectory_ReturnsMemoryFsCurrentDirectory() {
        // Arrange
        var fileSystem = new MemoryFileSystem(@"C:\Current\Path");
        var sut        = new MemoryDirectoryStatic(fileSystem);

        // Act
        var currentDirectory = sut.GetCurrentDirectory();

        // Assert
        currentDirectory.ShouldBe(@"C:\Current\Path");
    }

    [Fact]
    public void GetCurrentDirectory_ThrowsForNonMemoryFs() {
        // Arrange
        var fileSystem = new ZipFileSystem();
        var sut        = new MemoryDirectoryStatic(fileSystem);

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => sut.GetCurrentDirectory())
            .Message.ShouldBe($"Only {typeof(MemoryFileSystem)} supports concept of '{nameof(MemoryFileSystem.CurrentDirectory)}'.");
    }

    [Fact]
    public void Mock_RecordCallsCorrectly() {
        // Arrange
        var fileSystem = new MemoryFileSystem(@"C:\Path") {
            @"C:\Path\Foo",
            { @"C:\Path\Bar.txt", "File" },
            { @"C:\Path\Baz\Baz.txt", "File" }
        };
        var sut = new MemoryDirectoryStatic(fileSystem).Mock();

        // Act
        var exists = sut.Exists(@"C:\Path\Foo");
        var enumerateDirectories = sut.EnumerateDirectories(@"C:\Path");
        var currentDirectory = sut.GetCurrentDirectory();

        // Assert
        exists.ShouldBeTrue();
        enumerateDirectories.Count().ShouldBe(2);
        currentDirectory.ShouldBe(@"C:\Path");
        
        Received.InOrder(() => {
            sut.Exists(@"C:\Path\Foo");
            sut.EnumerateDirectories(@"C:\Path");
            sut.GetCurrentDirectory();
        });
    }
}
