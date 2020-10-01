using UGCS.TelemetryViewer.Helpers;

namespace UGCS.TelemetryViewer.Services
{
    public interface IAppContextContainer
    {
        public AppContext Context { get; }
    }
}
