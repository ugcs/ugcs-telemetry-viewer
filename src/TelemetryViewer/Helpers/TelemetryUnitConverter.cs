using System;
using System.Linq;
using UGCS.Sdk.Protocol.Encoding;
using UGCS.UcsServices;

namespace UGCS.TelemetryViewer.Helpers
{
    public static class TelemetryUnitConverter
    {
        private static readonly Semantic[] DEGREE_VALUES =
        {
            Semantic.S_HEADING,
            Semantic.S_ROLL,
            Semantic.S_PITCH,
            Semantic.S_YAW,
            Semantic.S_LATITUDE,
            Semantic.S_LONGITUDE
        };

        public static double? Convert(TelemetryKey key, double? value)
        {
            // Don't check, return as is
            if (value == null)
                return null;

            // Try to get field data, if none, return as is
            TelemetryField field = TelemetryKeys.GetTelemetryFieldByKeyOrNull(key);
            if (field == null)
                return value;

            // We have a semantic, check if it is to be coverted to degrees
            if (field.SemanticSpecified && DEGREE_VALUES.Contains(field.Semantic))
            {
                return value.Value / Math.PI * 180.0;
            }

            // Return as is
            return value;
        }
    }
}
