using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Input;
using WritingApp.ViewModels;
using System.Threading.Tasks;

namespace WritingApp.Views;

public partial class MainWindow : Window
{
    private MainViewModel? ViewModel => DataContext as MainViewModel;

    public MainWindow()
    {
        InitializeComponent();

        // 1. Text changed inside editor updates ViewModel
        Editor.TextChanged += (s, e) =>
        {
            ViewModel?.UpdateContent(Editor.Text);
        };

        // 2. Switching files updates editor text
        DataContextChanged += (s, e) =>
        {
            if (ViewModel != null)
            {
                ViewModel.PropertyChanged += (vs, ve) =>
                {
                    if (ve.PropertyName == nameof(MainViewModel.SelectedChapter))
                    {
                        if (ViewModel.SelectedChapter != null && Editor.Text != ViewModel.SelectedChapter.Content)
                        {
                            Editor.Text = ViewModel.SelectedChapter.Content;
                        }
                    }
                };

                if (ViewModel.SelectedChapter != null)
                {
                    Editor.Text = ViewModel.SelectedChapter.Content;
                }
            }
        };
    }

    // Handles the "Open Folder" button using Avalonia's StorageProvider
    private async void OpenFolderButton_Click(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Open Manuscript Folder",
            AllowMultiple = false
        });

        if (folders.Count > 0)
        {
            // Get the local filesystem path from the picked folder
            var folderPath = folders[0].TryGetLocalPath();
            if (!string.IsNullOrEmpty(folderPath) && ViewModel != null)
            {
                await ViewModel.LoadFolderAsync(folderPath);
            }
        }
    }

    // Handles manual save
    private async void SaveButton_Click(object? sender, RoutedEventArgs e)
    {
        if (ViewModel != null)
        {
            await ViewModel.SaveCurrentChapterAsync();
        }
    }


    private async void CreateChapterButton_Click(object? sender, RoutedEventArgs e)
    {
        await ExecuteCreateChapter();
    }
    // Allows pressing Enter inside the textbox to create the file
    private async void NewChapterInput_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            await ExecuteCreateChapter();
        }
    }

    private async Task ExecuteCreateChapter()
    {
        if (ViewModel == null) return;

        string chapterName = NewChapterInput.Text?.Trim() ?? string.Empty;
        if (!string.IsNullOrEmpty(chapterName))
        {
            await ViewModel.CreateChapterAsync(chapterName);
            NewChapterInput.Text = string.Empty; // Reset input field
        }
    }
}
