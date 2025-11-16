using System;
using System.Linq;
using Shouldly;

namespace MockFileSystem.Tests;

public class TestsZipFileSystem
{
    [Fact]
    public void ThrowsAbsolutePaths() {
        // Arrange
        var sut = new ZipFileSystem();

        // Act
        var act = () => sut.Add(@"C:\");

        // Assert
        act.ShouldThrow<ArgumentException>().Message.ShouldBe("Zip file do not support absolute paths.");
    }
    
    [Fact]
    public void RemoveLeadingSlash() {
        // Arrange
        var sut = new ZipFileSystem();

        // Act
        sut.Add(@"\Foo");

        // Assert
        var entries = sut.ToArray();
        entries.Length.ShouldBe(1);
        entries[0].Path.ShouldBe("Foo");
    }
    
    [Fact]
    public void Add_AddParents() {
        // Arrange
        var sut = new ZipFileSystem();
        
        // Act
        sut.Add(@"Path\Nested");
        
        // Assert
        var entries = sut.ToArray();
        entries.Length.ShouldBe(2);
        entries[0].Path.ShouldBe("Path");
        entries[1].Path.ShouldBe("Path/Nested");
    }
}
