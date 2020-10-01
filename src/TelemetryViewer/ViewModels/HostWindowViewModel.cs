using ReactiveUI;

namespace UGCS.TelemetryViewer.ViewModels
{
    public class HostWindowViewModel : ViewModelBase, IHostWindowViewModel
    {
        private string _host;
        public string Host
        {
            get => _host;
            set => this.RaiseAndSetIfChanged(ref _host, value);
        }

        private int? _port;
        public int? Port
        {
            get => _port;
            set => this.RaiseAndSetIfChanged(ref _port, value);
        }

        private string _message;
        public string Message
        {
            get => _message;
            set => this.RaiseAndSetIfChanged(ref _message, value);
        }

        private string _detailMessage;
        public string DetailMessage
        {
            get => _detailMessage;
            set => this.RaiseAndSetIfChanged(ref _detailMessage, value);
        }
    }
}
