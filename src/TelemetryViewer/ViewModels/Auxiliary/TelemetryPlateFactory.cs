namespace UGCS.TelemetryViewer.ViewModels.Auxiliary
{
    public class TelemetryPlateFactory : ITelemetryPlateFactory
    {

        public ITelemetryPlate Create(string Name, string TlemetryCode, string units,
            double? minThreshold, double? maxThreshold, int decimalPlaces)
        {
            ITelemetryPlate plate = new TelemetryPlate()
            {
                PlateName = Name,
                Units = units,
                TelemetryKeyCode = TlemetryCode,
                MinThreshold = minThreshold,
                MaxThreshold = maxThreshold,
                DecimalPlaces = decimalPlaces,
            };
            return plate;
        }

    }
}
