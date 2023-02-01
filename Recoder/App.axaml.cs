using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
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

                if (window.Screens.Primary is null)
                {
                    throw new PlatformNotSupportedException();
                }

                int positionX = Convert.ToInt32(window.Screens.Primary.Bounds.Width / 2 - window.Width / 2);
                window.Position = new PixelPoint(positionX, 0);

                desktop.MainWindow = window;
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
