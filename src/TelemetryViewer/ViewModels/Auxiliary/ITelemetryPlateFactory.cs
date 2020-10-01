namespace UGCS.TelemetryViewer.ViewModels.Auxiliary
{
    public interface ITelemetryPlateFactory
    {
        public ITelemetryPlate Create(string Name, string TlemetryCode, string units,
            double? minThreshold, double? maxThreshold, int decimalPlaces);
    }
}
