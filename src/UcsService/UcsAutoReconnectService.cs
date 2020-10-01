using log4net;
using System;
using System.Threading.Tasks;

namespace UGCS.UcsServices
{
    /// <summary>
    /// Monitors connection to ucs. If disconnected the the service try to reconnect while connection is not established.
    /// </summary>
    public sealed class UcsAutoReconnectService : IDisposable
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(UcsAutoReconnectService));

        private readonly ConnectionService _ucsConnection;
        private readonly TimeSpan _retryInterval = new TimeSpan(0, 0, 10);
        private Uri _ucsAddress = null;
        private UcsCredentials _credentials = null;
        private bool _isDisposed = false;

        public bool IsEnabled { get; private set; } = false;

        public UcsAutoReconnectService(ConnectionService ucsConnection)
        {
            _ucsConnection = ucsConnection ?? throw new ArgumentNullException(nameof(ucsConnection));
        }

        public void Enable(Uri ucsAddress, UcsCredentials credentials)
        {
            if (IsEnabled)
                throw new InvalidOperationException("Already enabled.");

            _ucsAddress = ucsAddress ?? throw new ArgumentNullException(nameof(ucsAddress));
            _credentials = credentials ?? throw new ArgumentNullException(nameof(credentials));

            IsEnabled = true;

            if (!_ucsConnection.IsConnected)
                reconnectAsync().Wait();

            _ucsConnection.Disconnected += ucsConnection_onDisconnected;
        }

        public void Disable()
        {
            if (!IsEnabled)
                throw new InvalidOperationException("Not enabled.");

            _ucsAddress = null;
            _credentials = null;
            _ucsConnection.Disconnected -= ucsConnection_onDisconnected;

            IsEnabled = false;
        }

        private async void ucsConnection_onDisconnected(object sender, EventArgs e)
        {
            _log.InfoFormat("Connection with UCS lost, will try to reconnect in {0}.", _retryInterval);
            await reconnectAsync(_retryInterval);
        }

        private async Task reconnectAsync(TimeSpan? delay = null)
        {
            bool isConnectionEstablished = false;

            while (!isConnectionEstablished)
            {
                if (delay.HasValue)
                    await Task.Delay(delay.Value);

                try
                {
                    _log.Info("Reconnecting to UCS.");
                    await _ucsConnection.ConnectAsync(_ucsAddress, _credentials);
                    _log.Info("Connection to ucs restored.");
                    isConnectionEstablished = true;
                }
                catch (Exception err)
                {
                    if (_log.IsInfoEnabled)
                        _log.Info($"Reconnection to UCS failed. Will retry in {delay}.", err);
                }
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _ucsConnection.Disconnected -= ucsConnection_onDisconnected;

            _isDisposed = true;
        }
    }
}
