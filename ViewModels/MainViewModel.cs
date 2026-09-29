using System;
using System.Collections.ObjectModel;
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
    public partial int WordCount { get; set; }

    [ObservableProperty]
    public partial int CharCount { get; set; }

    public MainViewModel()
    {
        // Sample starter data for your manuscript
        Chapters.Add(new ChapterItem
        {
            Title = "Chapter 1: The Beginning",
            Content = "# Chapter 1\n\nThe night was cold and silent. Rain drummed against the windowpane."
        });

        Chapters.Add(new ChapterItem
        {
            Title = "Chapter 2: The Departure",
            Content = "# Chapter 2\n\nBy morning, all tracks had been washed away."
        });

        SelectedChapter = Chapters[0];
    }

    public void UpdateContent(string markdownText)
    {
        if (SelectedChapter != null)
        {
            SelectedChapter.Content = markdownText;
        }

        // Use Markdig to strip markdown syntax so we only count narrative words
        string plainText = Markdown.ToPlainText(markdownText);

        CharCount = plainText.Length;
        WordCount = string.IsNullOrWhiteSpace(plainText)
            ? 0
            : plainText.Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }
}