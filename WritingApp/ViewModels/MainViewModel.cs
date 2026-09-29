using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using WritingApp.Models;
using WritingApp.Services;

namespace WritingApp.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly IManuscriptService _manuscriptService;

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

    // 1. Primary Constructor: Used at runtime by the DI container
    public MainViewModel(IManuscriptService manuscriptService)
    {
        _manuscriptService = manuscriptService;
    }

    // 2. Design-time Constructor: Used by the Avalonia XAML previewer
    public MainViewModel() : this(new ManuscriptService())
    {
    }

    public async Task LoadFolderAsync(string folderPath)
    {
        try
        {
            CurrentFolderPath = folderPath;
            Chapters.Clear();

            // Delegated to the service
            var loadedChapters = await _manuscriptService.LoadChaptersFromFolderAsync(folderPath);
            foreach (var chapter in loadedChapters)
            {
                Chapters.Add(chapter);
            }

            StatusMessage = $"Loaded {Chapters.Count} chapter(s).";

            if (Chapters.Count > 0)
            {
                SelectedChapter = Chapters[0];
                UpdateContent(SelectedChapter.Content);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load folder: {ex.Message}";
        }
    }

    public async Task CreateChapterAsync(string rawTitle)
    {
        if (string.IsNullOrWhiteSpace(CurrentFolderPath))
        {
            StatusMessage = "Please open a folder first!";
            return;
        }

        try
        {
            // Delegated to the service
            var newChapter = await _manuscriptService.CreateChapterAsync(CurrentFolderPath, rawTitle);

            Chapters.Add(newChapter);
            SelectedChapter = newChapter;
            UpdateContent(newChapter.Content);

            StatusMessage = $"Created '{newChapter.Title}'";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    public async Task SaveCurrentChapterAsync()
    {
        if (SelectedChapter == null) return;

        try
        {
            // Delegated to the service
            await _manuscriptService.SaveChapterAsync(SelectedChapter);
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

        // Delegated to the service
        WordCount = _manuscriptService.CalculateWordCount(markdownText);
        CharCount = markdownText?.Length ?? 0;
    }
}