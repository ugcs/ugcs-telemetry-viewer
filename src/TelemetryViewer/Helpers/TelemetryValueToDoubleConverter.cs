using UGCS.UcsServices.DTO;

namespace UGCS.TelemetryViewer.Helpers
{
    public static class TelemetryValueToDoubleConverter
    {
        public static double? Convert(TelemetryValue telemetry)
        {
            if (telemetry == null)
                return null;

            if (telemetry.DoubleValueSpecified)
                return telemetry.DoubleValue;

            if (telemetry.BoolValueSpecified)
                return telemetry.BoolValue ? 1 : 0;

            if (telemetry.FloatValueSpecified)
                return telemetry.FloatValue;

            if (telemetry.IntValueSpecified)
                return telemetry.IntValue;

            if (telemetry.LongValueSpecified)
                return telemetry.LongValue;

            if (telemetry.StringValueSpecified)
            {
                if (double.TryParse(telemetry.StringValue, out double result))
                {
                    return result;
                }
                else
                {
                    return double.NaN;
                }
            }

            return null;
        }
    }
}
