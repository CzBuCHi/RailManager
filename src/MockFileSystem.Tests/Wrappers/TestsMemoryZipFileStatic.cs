using System.IO;
using System.Linq;
using MockFileSystem.Entries;
using MockFileSystem.Wrappers;
using NSubstitute;
using Shouldly;

namespace MockFileSystem.Tests.Wrappers;

public class TestsMemoryZipFileStatic
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ExtractToDirectory_ThrowWhenSourceNotFoundOrDirectory(bool isDirectory) {
        // Arrange
        var fileSystem = new MemoryFileSystem();
        if (isDirectory) {
            fileSystem.Add(@"C:\Source.zip");
        }

        var sut = new MemoryZipFileStatic(fileSystem);

        // Act & Assert
        Should.Throw<FileNotFoundException>(() => sut.ExtractToDirectory(@"C:\Source.zip", @"C:\target"))
            .Message.ShouldBe(@"File 'C:\Source.zip' not found.");
    }

    [Fact]
    public void ExtractToDirectory_ThrowWhenSourceNotZip() {
        // Arrange
        var fileSystem = new MemoryFileSystem();
        fileSystem.Add(@"C:\Source.zip", [1, 2, 3]);
        var sut = new MemoryZipFileStatic(fileSystem);

        // Act & Assert
        Should.Throw<InvalidDataException>(() => sut.ExtractToDirectory(@"C:\Source.zip", @"C:\target"))
            .Message.ShouldBe(@"File 'C:\Source.zip' is not ZIP.");
    }

    [Fact]
    public void ExtractToDirectory_CreatesCorrectEntries() {
        // Arrange
        var zipFile = new ZipFileSystem();
        zipFile.Add(@"Path\In\Zip\File.txt", [1, 2, 3]);

        var fileSystem = new MemoryFileSystem();
        fileSystem.Add(@"C:\Real\Path\File.zip", zipFile);
        var sut = new MemoryZipFileStatic(fileSystem);

        // Act
        sut.ExtractToDirectory(@"C:\Real\Path\File.zip", @"C:\Real\Path\Dest");

        // Assert
        var entries = fileSystem.ToArray();
        entries.Length.ShouldBe(9);
        entries[0].Path.ShouldBe(@"C:\");
        entries[1].Path.ShouldBe(@"C:\Real");
        entries[2].Path.ShouldBe(@"C:\Real\Path");
        entries[3].Path.ShouldBe(@"C:\Real\Path\Dest");
        entries[4].Path.ShouldBe(@"C:\Real\Path\Dest\Path");
        entries[5].Path.ShouldBe(@"C:\Real\Path\Dest\Path\In");
        entries[6].Path.ShouldBe(@"C:\Real\Path\Dest\Path\In\Zip");
        entries[7].Path.ShouldBe(@"C:\Real\Path\Dest\Path\In\Zip\File.txt");
        entries[8].Path.ShouldBe(@"C:\Real\Path\File.zip");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void OpenRead_ThrowWhenSourceNotFoundOrDirectory(bool isDirectory) {
        // Arrange
        var fileSystem = new MemoryFileSystem();
        if (isDirectory) {
            fileSystem.Add(@"C:\Source.zip");
        }

        var sut = new MemoryZipFileStatic(fileSystem);

        // Act & Assert
        Should.Throw<FileNotFoundException>(() => sut.OpenRead(@"C:\Source.zip"))
            .Message.ShouldBe(@"File 'C:\Source.zip' not found.");
    }

    [Fact]
    public void OpenRead_ReturnsCorrectZipArchive() {
        // Arrange
        byte[] content = [1, 2, 3];

        var zipFile = new ZipFileSystem();
        zipFile.Add(@"Path\In\Zip\File.txt", content);

        var fileSystem = new MemoryFileSystem();
        fileSystem.Add(@"C:\Real\Path\File.zip", zipFile);

        var sut = new MemoryZipFileStatic(fileSystem);

        // Act
        var zipArchive = sut.OpenRead(@"C:\Real\Path\File.zip");

        // Assert
        zipArchive.ShouldNotBeNull();
        zipArchive.Entries.Count.ShouldBe(1);
        var entry = zipArchive.GetEntry("Path/In/Zip/File.txt");
        entry.ShouldNotBeNull();
        entry.Name.ShouldBe("File.txt");
        entry.Open().ShouldBeOfType<MemoryStream>().ToArray().ShouldBeEquivalentTo(content);
    }
    
    [Fact]
    public void Mock_RecordCallsCorrectly() {
        // Arrange
        var fileSystem = new MemoryFileSystem(@"C:\Path") {
            { @"C:\Path\Bar.zip", new ZipFileSystem() { { "File.txt", "File" } } },
        };
        var sut = new MemoryZipFileStatic(fileSystem).Mock();

        // Act
        sut.ExtractToDirectory(@"C:\Path\Bar.zip", @"C:\Path\Bar");
        var openRead = sut.OpenRead(@"C:\Path\Bar.zip");

        // Assert
        fileSystem.FindEntry<MemoryBinaryFileEntry>(@"C:\Path\Bar\File.txt").ShouldNotBeNull().StringContent.ShouldBe("File");
        openRead.ShouldNotBeNull().GetEntry("File.txt").ShouldNotBeNull().FullName.ShouldBe("File.txt");
        
        Received.InOrder(() => {
            sut.ExtractToDirectory(@"C:\Path\Bar.zip", @"C:\Path\Bar");
            sut.OpenRead(@"C:\Path\Bar.zip");
        });
    }
}
