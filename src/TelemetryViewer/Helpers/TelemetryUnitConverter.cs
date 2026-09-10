using System;
using System.Linq;
using Com.Ugcs.Ucs.Proto;
using UGCS.UcsServices;

namespace UGCS.TelemetryViewer.Helpers
{
    public static class TelemetryUnitConverter
    {
        private static readonly Semantic[] DEGREE_VALUES =
        {
            Semantic.SHeading,
            Semantic.SRoll,
            Semantic.SPitch,
            Semantic.SYaw,
            Semantic.SLatitude,
            Semantic.SLongitude
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
            if (field.HasSemantic && DEGREE_VALUES.Contains(field.Semantic))
            {
                return value.Value / Math.PI * 180.0;
            }

            // Return as is
            return value;
        }
    }
}
