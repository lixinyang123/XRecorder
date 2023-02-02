using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Recoder.ViewModels;
using Recoder.Views;
using System;

namespace Recoder
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                MainWindow window = new()
                {
                    DataContext = new MainWindowViewModel()
                };

                Screen primaryScreen = window.Screens.Primary ?? throw new PlatformNotSupportedException();
                int positionX = Convert.ToInt32(primaryScreen.Bounds.Width / 2 - window.Width / 2);
                window.Position = new PixelPoint(positionX, 0);

                desktop.MainWindow = window;
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
