using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using UGCS.TelemetryViewer.ViewModels;
using UGCS.UcsServices;

namespace UGCS.TelemetryViewer.Views
{
    public class LoginWindow : Window
    {
        public LoginWindow()
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
            UcsCredentials ucsCredentials = new UcsCredentials(viewModel.Login, viewModel.Password);

            // ***********************
            // Avalonia bug?
            // I don't know a reason, but app will crash if close this window without manually focus it.
            // Probably it happens cause non-trivial our custom text box logic.
            Focus();
            // ***********************

            this.Close(ucsCredentials);
        }

        public void Close(object sender, RoutedEventArgs args)
        {
            this.Close(null);
        }

        private ILoginWindowViewModel getViewModel()
        {
            return (ILoginWindowViewModel)DataContext;
        }
    }
}
