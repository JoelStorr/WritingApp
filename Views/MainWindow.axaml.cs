using Avalonia.Controls;
using WritingApp.ViewModels;

namespace WritingApp.Views; // <-- Must match the namespace in x:Class

public partial class MainWindow : Window
{
    private MainViewModel? ViewModel => DataContext as MainViewModel;

    public MainWindow()
    {
        InitializeComponent();

        Editor.TextChanged += (s, e) =>
        {
            ViewModel?.UpdateContent(Editor.Text);
        };

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
}