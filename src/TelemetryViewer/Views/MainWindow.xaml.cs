using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using UGCS.TelemetryViewer.ViewModels;

namespace UGCS.TelemetryViewer.Views
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            initializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void initializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void ResetPosition()
        {
            if (DataContext != null)
                Position = ((IMainWindowViewModel)DataContext).Position;
            this.PositionChanged += onPosChanged;
        }

        private void onPosChanged(object sender, PixelPointEventArgs args)
        {
            ((IMainWindowViewModel)DataContext).Position = args.Point;
        }
    }
}
