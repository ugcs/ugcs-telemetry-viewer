using Com.Ugcs.Ucs.Proto;

namespace UGCS.UcsServices.DTO
{
    public class TelemetryValue
    {
        public int Id { get; set; }
        public bool IdSpecified { get; set; }
        public int Version { get; set; }
        public bool VersionSpecified { get; set; }
        public bool BoolValue { get; set; }
        public bool BoolValueSpecified { get; set; }
        public int IntValue { get; set; }
        public bool IntValueSpecified { get; set; }
        public long LongValue { get; set; }
        public bool LongValueSpecified { get; set; }
        public float FloatValue { get; set; }
        public bool FloatValueSpecified { get; set; }
        public double DoubleValue { get; set; }
        public bool DoubleValueSpecified { get; set; }
        public string StringValue { get; set; }
        public bool StringValueSpecified { get; set; }
        public string Tag { get; set; }
        public bool TagSpecified { get; set; }

        public static TelemetryValue MapValue(Value v)
        {
            if (v == null)
            {
                return null;
            }
            return new TelemetryValue()
            {
                BoolValue = v.BoolValue,
                BoolValueSpecified = v.HasBoolValue,
                DoubleValue = v.DoubleValue,
                DoubleValueSpecified = v.HasDoubleValue,
                FloatValue = v.FloatValue,
                FloatValueSpecified = v.HasFloatValue,
                Id = v.Id,
                IdSpecified = v.HasId,
                IntValue = v.IntValue,
                IntValueSpecified = v.HasIntValue,
                LongValue = v.LongValue,
                LongValueSpecified = v.HasLongValue,
                StringValue = v.StringValue,
                StringValueSpecified = v.HasStringValue,
                Tag = v.Tag,
                TagSpecified = v.HasTag,
                Version = v.Version,
                VersionSpecified = v.HasVersion
            };
        }
    }
}
