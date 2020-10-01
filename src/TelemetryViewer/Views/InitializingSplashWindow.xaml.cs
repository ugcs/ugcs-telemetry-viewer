using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace UGCS.TelemetryViewer.Views
{
    public class InitializingSplashWindow : Window
    {
        public InitializingSplashWindow()
        {
            this.initializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void initializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
