using UGCS.Sdk.Protocol.Encoding;

namespace UGCS.UcsServices.DTO
{
    public class TelemetryValue : Value
    {
        public static TelemetryValue MapValue(Value v)
        {
            if (v == null)
            {
                return null;
            }
            return new TelemetryValue()
            {
                BoolValue = v.BoolValue,
                BoolValueSpecified = v.BoolValueSpecified,
                DoubleValue = v.DoubleValue,
                DoubleValueSpecified = v.DoubleValueSpecified,
                FloatValue = v.FloatValue,
                FloatValueSpecified = v.FloatValueSpecified,
                Id = v.Id,
                IdSpecified = v.IdSpecified,
                IntValue = v.IntValue,
                IntValueSpecified = v.IntValueSpecified,
                LongValue = v.LongValue,
                LongValueSpecified = v.LongValueSpecified,
                StringValue = v.StringValue,
                StringValueSpecified = v.StringValueSpecified,
                Tag = v.Tag,
                TagSpecified = v.TagSpecified,
                Version = v.Version,
                VersionSpecified = v.VersionSpecified
            };
        }
    }
}
