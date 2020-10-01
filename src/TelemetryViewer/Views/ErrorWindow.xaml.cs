using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace UGCS.TelemetryViewer.Views
{
    public class ErrorWindow : Window
    {
        public ErrorWindow()
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
