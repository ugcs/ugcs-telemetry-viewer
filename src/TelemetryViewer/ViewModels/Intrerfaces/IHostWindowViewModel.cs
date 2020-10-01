namespace UGCS.TelemetryViewer.ViewModels
{
    public interface IHostWindowViewModel
    {
        string Message { get; set; }

        string DetailMessage { get; set; }

        string Host { get; set; }

        int? Port { get; set; }

    }
}
