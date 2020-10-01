namespace UGCS.TelemetryViewer.ViewModels
{
    public interface IInitializingSplashWindowViewModel
    {

        void StartInitializing();

        string Status { get; }

    }
}
