using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using UGCS.TelemetryViewer.ViewModels;

namespace UGCS.TelemetryViewer.Views
{
    public class HostWindow : Window
    {
        private const string URI_SCHEME = "tcp";

        public HostWindow()
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

        public void TryCloseWithResult(object sender, RoutedEventArgs args)
        {
            var viewModel = getViewModel();
            if (viewModel.Host != null && viewModel.Port > 0 && viewModel.Port < 65356 &&
                Uri.TryCreate($"{URI_SCHEME}://{viewModel.Host}:{viewModel.Port}", UriKind.RelativeOrAbsolute, out Uri uri))
            {
                this.Close(uri);
            }
            else
            {
                viewModel.Message = "Invalid host or port format";
            }
        }

        public void Close(object sender, RoutedEventArgs args)
        {
            this.Close(null);
        }

        private IHostWindowViewModel getViewModel()
        {
            return (IHostWindowViewModel)DataContext;
        }

    }
}
