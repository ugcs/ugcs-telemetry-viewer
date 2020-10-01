using Avalonia.Controls;
using log4net;
using ReactiveUI;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using UGCS.SsdpDiscoveryService;
using UGCS.TelemetryViewer.Views;
using UGCS.UcsServices;

namespace UGCS.TelemetryViewer.ViewModels
{
    public class InitializingSplashWindowViewModel : ViewModelBase, IInitializingSplashWindowViewModel
    {
        private class ConnectAsyncResult
        {
            /// <summary>
            /// True if connection established.
            /// Otherwise false.
            /// </summary>
            public bool Success { get; }

            /// <summary>
            /// Service address, connection successfullly established with.
            /// Null if connection failed.
            /// </summary>
            public Uri Address { get; }
            /// <summary>
            /// Credentials, connection successfullly established with.
            /// Null if connection failed.
            /// </summary>
            public UcsCredentials Credentials { get; }

            public ConnectAsyncResult(bool success, Uri address, UcsCredentials creds)
            {
                Success = success;
                Address = address;
                Credentials = creds;
            }
        }

        private const string UCS_SERVER_TYPE = "ugcs:hci-server";
        private readonly TimeSpan DISCOVERY_TIMEOUT = new TimeSpan(0, 0, 10);

        private const string DEFAULT_LOGIN = "";
        private const string DEFAULT_PASSWORD = "";

        private static readonly ILog _log = LogManager.GetLogger(typeof(InitializingSplashWindowViewModel));

        private readonly IDiscoveryService _discoveryService;
        private readonly IVisualComponentsFactory _viewFactory;
        private readonly ConnectionService _connectionService;
        private readonly UcsAutoReconnectService _reconnectionService;

        public InitializingSplashWindowViewModel(IDiscoveryService discoveryService,
            IVisualComponentsFactory viewFactory, ConnectionService connectionService,
            UcsAutoReconnectService reconnectionService)
        {
            _connectionService = connectionService ?? throw new ArgumentNullException(nameof(connectionService));
            _viewFactory = viewFactory ?? throw new ArgumentNullException(nameof(viewFactory));
            _discoveryService = discoveryService ?? throw new ArgumentNullException(nameof(discoveryService));
            _reconnectionService = reconnectionService ?? throw new ArgumentNullException(nameof(reconnectionService));
        }

        private string _status;
        public string Status
        {
            get => _status;
            private set => this.RaiseAndSetIfChanged(ref _status, value);
        }

        public async void StartInitializing()
        {
            Status = "Discovering...";

            Uri ucsAddress = await getUcsAddress();
            if (ucsAddress == null)
            {
                TelemetryViewerApp.Current.Desktop.MainWindow.Close();
                return;
            }

            Status = $"Connecting to {ucsAddress} ...";

            var credentials = new UcsCredentials(DEFAULT_LOGIN, DEFAULT_PASSWORD);
            ConnectAsyncResult connectRetuls;
            try
            {
                connectRetuls = await tryConnectUntilSuccessOrUserCancel(ucsAddress, credentials);
                if (!connectRetuls.Success)
                {
                    TelemetryViewerApp.Current.Desktop.Shutdown();
                    return;
                }
            }
            catch (Exception err)
            {
                _log.Error("Connection failed with unknown error. Application will be terminated.", err);
                ErrorWindow errorDialog = _viewFactory.CreateWindow<ErrorWindow>(true);
                errorDialog.DataContext = err;
                await errorDialog.ShowDialog(TelemetryViewerApp.Current.Desktop.MainWindow);
                TelemetryViewerApp.Current.Desktop.Shutdown();
                return;
            }

            _reconnectionService.Enable(connectRetuls.Address, connectRetuls.Credentials);

            switchToMainWindow();
        }

        private void switchToMainWindow()
        {
            MainWindow mainWindow = _viewFactory.CreateWindow<MainWindow>();
            mainWindow.Show();
            mainWindow.ResetPosition();
            Window currentWindow = TelemetryViewerApp.Current.Desktop.MainWindow;
            TelemetryViewerApp.Current.SetMainWindow(mainWindow);
            currentWindow.Close();
        }

        private async Task<ConnectAsyncResult> tryConnectUntilSuccessOrUserCancel(Uri initialUcsAddress, UcsCredentials initialCredentials)
        {
            Uri ucsAddress = initialUcsAddress;
            UcsCredentials credentials = initialCredentials;

            while (true)
            {
                try
                {
                    Status = $"Connecting to {ucsAddress} ...";
                    await _connectionService.ConnectAsync(ucsAddress, credentials);
                }
                catch (LoginException)
                {
                    _log.Debug("Ucs authentication failed. New credentials requested from user.");

                    string titleMessage = buildLoginTitleMessage(credentials);
                    credentials = await showLoginDialog(titleMessage, credentials);
                    if (credentials == null)
                        return new ConnectAsyncResult(false, null, null);
                    continue;
                }
                catch (Exception err) when (err is SocketException || err is ConnectionException || err is TimeoutException)
                {
                    _log.Debug("Ucs connection failed. Address confirmation dialog is displayed to user.");

                    ucsAddress = await showHostDialog("Connection to UCS-server failed", ucsAddress, err.Message);
                    if (ucsAddress == null)
                        return new ConnectAsyncResult(false, null, null);
                    continue;
                }

                return new ConnectAsyncResult(true, ucsAddress, credentials);
            }
        }

        private async Task<Uri> getUcsAddress()
        {
            _log.Info("Ucs auto discovering used.");
            Uri ucsAddress = await _discoveryService.TryFoundAsync(UCS_SERVER_TYPE, DISCOVERY_TIMEOUT);

            if (ucsAddress == null)
            {
                if (_log.IsInfoEnabled)
                    _log.InfoFormat("Ucs wasn't discovered within {0} seconds. Address requested from user.", DISCOVERY_TIMEOUT);
                ucsAddress = await showHostDialog("UCS-server not found", null);
            }

            return ucsAddress;
        }

        private async Task<Uri> showHostDialog(string message, Uri displayUri, string detailMessage = null)
        {
            HostWindow hostWindow = _viewFactory.CreateWindow<HostWindow>();
            ((IHostWindowViewModel)hostWindow.DataContext).Message = message;
            ((IHostWindowViewModel)hostWindow.DataContext).DetailMessage = detailMessage;
            if (displayUri != null)
            {
                ((IHostWindowViewModel)hostWindow.DataContext).Host = displayUri.Host;
                ((IHostWindowViewModel)hostWindow.DataContext).Port = displayUri.Port;
            }
            return await hostWindow.ShowDialog<Uri>(TelemetryViewerApp.Current.Desktop.MainWindow);
        }

        private async Task<UcsCredentials> showLoginDialog(string message, UcsCredentials displayCredentials)
        {
            LoginWindow loginWindow = _viewFactory.CreateWindow<LoginWindow>();
            ((ILoginWindowViewModel)loginWindow.DataContext).Message = message;
            if (displayCredentials != null)
            {
                ((ILoginWindowViewModel)loginWindow.DataContext).Login = displayCredentials.Login;
                ((ILoginWindowViewModel)loginWindow.DataContext).Password = displayCredentials.Password;
            }
            return await loginWindow.ShowDialog<UcsCredentials>(TelemetryViewerApp.Current.Desktop.MainWindow);
        }

        private static string buildLoginTitleMessage(UcsCredentials credentials)
        {
            return (string.IsNullOrEmpty(credentials.Login) && string.IsNullOrEmpty(credentials.Password)) ?
                                "Enter your login and password" :
                                "Wrong login or password";
        }
    }
}
