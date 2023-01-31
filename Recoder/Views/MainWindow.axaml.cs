using Avalonia.Controls;
using System;

namespace Recoder.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            if (Screens.Primary is null)
            {
                throw new PlatformNotSupportedException();
            }

            int positionX = Convert.ToInt32(Screens.Primary.Bounds.Width / 2 - Width / 2);
            Position = new Avalonia.PixelPoint(positionX, 0);
        }
    }
}
