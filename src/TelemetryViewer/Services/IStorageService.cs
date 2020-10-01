using UGCS.TelemetryViewer.Helpers;

namespace UGCS.TelemetryViewer.Services
{
    public interface IStorageService
    {
        void StoreAppContext(AppContext appContext);

        bool TryLoadAppContext(out AppContext appContext);
    }
}
