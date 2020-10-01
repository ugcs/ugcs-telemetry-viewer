using System.Collections.Concurrent;
using UGCS.UcsServices;
using UGCS.UcsServices.DTO;

namespace UGCS.TelemetryViewer.Models
{
    public interface IClientVehicle
    {
        int Id { get; }
        bool IsConnected { get; }
        string Name { get; }
        event System.EventHandler<bool> IsConnectChange;
        ConcurrentDictionary<TelemetryKey, TelemetryValue> Telemetry { get; }
    }
}