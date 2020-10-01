using UGCS.UcsServices;
using UGCS.UcsServices.DTO;

namespace UGCS.TelemetryViewer.Helpers
{
    public class TelemetryData
    {
        public int VehicleId { get; set; }
        public TelemetryKey TelemetryKey { get; set; }
        public TelemetryValue TelemetryValue { get; set; }
    }
}
