using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using WritingApp.Services;
using Xunit;

namespace WritingApp.Tests;

public class ManuscriptServiceTests
{
    private readonly ManuscriptService _service = new();

    [Theory]
    [InlineData("# Hello World", 2)]
    [InlineData("## A Title\n\nThis is a **paragraph**.", 6)]
    [InlineData("", 0)]
    public void CalculateWordCount_ShouldIgnoreMarkdownFormatting(string markdown, int expectedWords)
    {
        int count = _service.CalculateWordCount(markdown);
        count.Should().Be(expectedWords);
    }

    [Fact]
    public async Task CreateChapterAsync_ShouldCreateRealFileOnDisk()
    {
        // Arrange
        string tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Act
            var chapter = await _service.CreateChapterAsync(tempDir, "Chapter 1");

            // Assert
            File.Exists(chapter.FilePath).Should().BeTrue();
            chapter.Title.Should().Be("Chapter 1");
            chapter.Content.Should().Contain("# Chapter 1");
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }
}
