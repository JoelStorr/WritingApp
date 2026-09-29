using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using WritingApp.Services;
using WritingApp.ViewModels;
using WritingApp.Views;

namespace WritingApp;

public partial class App : Application
{
    // Access the service provider globally if ever needed
    public IServiceProvider? Services { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // 1. Register all services and ViewModels
        var collection = new ServiceCollection();

        // Services (Singletons live for the entire lifetime of the app)
        collection.AddSingleton<IManuscriptService, ManuscriptService>();

        // ViewModels
        collection.AddTransient<MainViewModel>();

        // Build container
        Services = collection.BuildServiceProvider();

        // 2. Resolve MainViewModel through DI and pass it to MainWindow
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainViewModel = Services.GetRequiredService<MainViewModel>();

            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
