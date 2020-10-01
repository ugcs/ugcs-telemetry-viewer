using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace UGCS.TelemetryViewer.Views
{
    public class TelemetryPlateControl : UserControl
    {
        public TelemetryPlateControl()
        {
            this.initializeComponent();
        }

        private void initializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
