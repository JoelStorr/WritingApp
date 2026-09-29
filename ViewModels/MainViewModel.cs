using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Markdig;
using WritingApp.Models;

namespace WritingApp.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial ObservableCollection<ChapterItem> Chapters { get; set; } = new();

    [ObservableProperty]
    public partial ChapterItem? SelectedChapter { get; set; }

    [ObservableProperty]
    public partial string? CurrentFolderPath { get; set; }

    [ObservableProperty]
    public partial int WordCount { get; set; }

    [ObservableProperty]
    public partial int CharCount { get; set; }

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = "Ready";

    public async Task LoadFolderAsync(string folderPath)
    {
        if (!Directory.Exists(folderPath)) return;

        CurrentFolderPath = folderPath;
        Chapters.Clear();

        // Scan folder for Markdown files (.md)
        var files = Directory.GetFiles(folderPath, "*.md", SearchOption.TopDirectoryOnly)
                             .OrderBy(f => f);

        foreach (var file in files)
        {
            var fileInfo = new FileInfo(file);
            var content = await File.ReadAllTextAsync(file);

            Chapters.Add(new ChapterItem
            {
                Title = Path.GetFileNameWithoutExtension(fileInfo.Name),
                FilePath = file,
                Content = content
            });
        }

        StatusMessage = $"Loaded {Chapters.Count} chapter(s).";

        // Select the first chapter automatically
        if (Chapters.Count > 0)
        {
            SelectedChapter = Chapters[0];
            UpdateContent(SelectedChapter.Content);
        }
    }

    public async Task SaveCurrentChapterAsync()
    {
        if (SelectedChapter == null || string.IsNullOrEmpty(SelectedChapter.FilePath))
            return;

        try
        {
            await File.WriteAllTextAsync(SelectedChapter.FilePath, SelectedChapter.Content);
            StatusMessage = $"Saved: {SelectedChapter.Title}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Save error: {ex.Message}";
        }
    }

    public void UpdateContent(string markdownText)
    {
        if (SelectedChapter != null)
        {
            SelectedChapter.Content = markdownText;
        }

        string plainText = Markdown.ToPlainText(markdownText ?? string.Empty);

        CharCount = plainText.Length;
        WordCount = string.IsNullOrWhiteSpace(plainText)
            ? 0
            : plainText.Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }

    public async Task CreateChapterAsync(string rawTitle)
    {
        if (string.IsNullOrWhiteSpace(CurrentFolderPath))
        {
            StatusMessage = "Please open a folder first!";
            return;
        }

        if (string.IsNullOrWhiteSpace(rawTitle))
        {
            StatusMessage = "Chapter name cannot be empty.";
            return;
        }

        // Sanitize illegal filename characters
        string sanitizedTitle = rawTitle.Trim();
        foreach (char invalidChar in Path.GetInvalidFileNameChars())
        {
            sanitizedTitle = sanitizedTitle.Replace(invalidChar, '_');
        }

        // Ensure it ends with .md
        string fileName = sanitizedTitle.EndsWith(".md", StringComparison.OrdinalIgnoreCase)
            ? sanitizedTitle
            : $"{sanitizedTitle}.md";

        string fullPath = Path.Combine(CurrentFolderPath, fileName);

        if (File.Exists(fullPath))
        {
            StatusMessage = $"'{fileName}' already exists!";
            return;
        }

        try
        {
            // Initial content (or keep string.Empty for a blank file)
            string initialContent = $"# {Path.GetFileNameWithoutExtension(fileName)}\n\n";
            await File.WriteAllTextAsync(fullPath, initialContent);

            var newChapter = new ChapterItem
            {
                Title = Path.GetFileNameWithoutExtension(fileName),
                FilePath = fullPath,
                Content = initialContent
            };

            Chapters.Add(newChapter);
            SelectedChapter = newChapter;
            UpdateContent(newChapter.Content);

            StatusMessage = $"Created '{fileName}'";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error creating file: {ex.Message}";
        }
    }
}