using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Markdig;
using WritingApp.Models;

namespace WritingApp.Services;

public class ManuscriptService : IManuscriptService
{
    public async Task<List<ChapterItem>> LoadChaptersFromFolderAsync(string folderPath)
    {
        if (!Directory.Exists(folderPath))
            throw new DirectoryNotFoundException($"Folder not found: {folderPath}");

        var chapters = new List<ChapterItem>();
        var files = Directory.GetFiles(folderPath, "*.md").OrderBy(f => f);

        foreach (var file in files)
        {
            var content = await File.ReadAllTextAsync(file);
            chapters.Add(new ChapterItem
            {
                Title = Path.GetFileNameWithoutExtension(file),
                FilePath = file,
                Content = content
            });
        }

        return chapters;
    }

    public async Task<ChapterItem> CreateChapterAsync(string folderPath, string chapterTitle)
    {
        if (!Directory.Exists(folderPath))
            throw new DirectoryNotFoundException("Project folder must exist.");

        string sanitized = string.Join("_", chapterTitle.Split(Path.GetInvalidFileNameChars())).Trim();
        if (!sanitized.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
            sanitized += ".md";

        string fullPath = Path.Combine(folderPath, sanitized);
        if (File.Exists(fullPath))
            throw new InvalidOperationException($"File '{sanitized}' already exists.");

        string defaultContent = $"# {Path.GetFileNameWithoutExtension(sanitized)}\n\n";
        await File.WriteAllTextAsync(fullPath, defaultContent);

        return new ChapterItem
        {
            Title = Path.GetFileNameWithoutExtension(sanitized),
            FilePath = fullPath,
            Content = defaultContent
        };
    }

    public async Task SaveChapterAsync(ChapterItem chapter)
    {
        if (string.IsNullOrEmpty(chapter.FilePath))
            throw new InvalidOperationException("Cannot save a chapter without a valid file path.");

        await File.WriteAllTextAsync(chapter.FilePath, chapter.Content);
    }

    public int CalculateWordCount(string markdownContent)
    {
        if (string.IsNullOrWhiteSpace(markdownContent)) return 0;

        string plainText = Markdown.ToPlainText(markdownContent);
        return plainText.Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }
}
