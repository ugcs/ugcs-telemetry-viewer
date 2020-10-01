using ReactiveUI;

namespace UGCS.TelemetryViewer.ViewModels
{
    public class LoginWindowViewModel : ViewModelBase, ILoginWindowViewModel
    {
        private string _login;
        public string Login
        {
            get => _login;
            set => this.RaiseAndSetIfChanged(ref _login, value);
        }

        private string _password;
        public string Password
        {
            get => _password;
            set => this.RaiseAndSetIfChanged(ref _password, value);
        }

        private string _message;
        public string Message
        {
            get => _message;
            set => this.RaiseAndSetIfChanged(ref _message, value);
        }
    }
}
