using System.Linq;
using MockFileSystem.Wrappers;
using NSubstitute;
using Shouldly;

namespace MockFileSystem.Tests.Wrappers;

public class TestsMemoryZipArchive
{
    [Fact]
    public void Entries_EnumerateCorrectly() {
        // Arrange
        var memoryZip = new ZipFileSystem {
            "Directory",
            { "File.txt", "Content" }
        };
        var sut = new MemoryZipArchive(memoryZip);

        // Act
        var actual = sut.Entries.ToArray();

        // Assert
        actual.Length.ShouldBe(1);
    }

    [Fact]
    public void GetEntry_ReturnsCorrectZipArchiveEntry() {
        // Arrange
        var memoryZip = new ZipFileSystem {
            "Directory",
            { "Path/File.txt", "Content" }
        };
        var sut = new MemoryZipArchive(memoryZip);

        // Act
        var actual = sut.GetEntry("Path/File.txt");

        // Assert
        actual.ShouldNotBeNull();
        actual.FullName.ShouldBe("Path/File.txt");
        actual.Name.ShouldBe("File.txt");
    }

    [Fact]
    public void GetEntry_ReturnNull_WhenNotFound() {
        // Arrange
        var memoryZip = new ZipFileSystem();
        var sut       = new MemoryZipArchive(memoryZip);

        // Act
        var actual = sut.GetEntry("Path/File.txt");

        // Assert
        actual.ShouldBeNull();
    }
    
    [Fact]
    public void Mock_CallBaseAndRecordCallsCorrectly() {
        // Arrange
        var memoryZip = new ZipFileSystem();
        var sut       = new MemoryZipArchive(memoryZip).Mock();

        // Act
        var actual = sut.GetEntry("Path/File.txt");

        // Assert
        actual.ShouldBeNull();
        sut.Received().GetEntry("Path/File.txt");
    }
}
