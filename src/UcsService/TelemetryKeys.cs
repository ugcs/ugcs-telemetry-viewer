using System;
using System.Collections.Generic;
using UGCS.Sdk.Protocol.Encoding;

namespace UGCS.UcsServices
{
    public struct TelemetryKey
    {
        public string ComplexCode { get; set; }

        private static string getSubsystemString(Subsystem subsystem)
        {
            switch (subsystem)
            {
                case Subsystem.S_CONTROL_SERVER: return "[srv]";
                case Subsystem.S_FLIGHT_CONTROLLER: return "[f]";
                case Subsystem.S_GIMBAL: return "[g]";
                case Subsystem.S_CAMERA: return "[cam]";
                case Subsystem.S_ADSB_TRANSPONDER: return "[a-t]";
                case Subsystem.S_WINCH: return "[w]";
                case Subsystem.S_HANGAR: return "[h]";
                case Subsystem.S_USER: return "[u]";
                case Subsystem.S_GPR: return "[gpr]";
                case Subsystem.S_ADSB_RECEIVER: return "[a-r]";
                case Subsystem.S_ADSB_VEHICLE: return "[a-v]";
                case Subsystem.S_WEATHER_STATION: return "[ws]";
                default: return "[-]";
            }
        }

        public TelemetryKey(string complexCode)
        {
            this.ComplexCode = complexCode;
        }

        public TelemetryKey(Subsystem subsystem, string code)
        {
            this.ComplexCode = getSubsystemString(subsystem) + " " + code;
        }

        public override int GetHashCode()
        {
            return ComplexCode.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is TelemetryKey key)
            {
                return this.ComplexCode == key.ComplexCode;
            }
            return false;
        }

        public override string ToString()
        {
            return ComplexCode;
        }
    }
    public static class TelemetryKeys
    {
        public static TelemetryField GetTelemetryFieldByKeyOrNull(TelemetryKey telemetryKey) {
            if (TelemetryFields.TryGetValue(telemetryKey, out TelemetryField telemetry))
                return telemetry;
            return null;
        }

        public static TelemetryField GetTelemetryFieldByKey(TelemetryKey telemetryKey)
        {
            return TelemetryFields[telemetryKey];
        }

        private static readonly Dictionary<TelemetryKey, TelemetryField> TelemetryFields = new Dictionary<TelemetryKey, TelemetryField>();

        public static readonly TelemetryKey UPLINK_ACTIVE = new TelemetryKey(Subsystem.S_FLIGHT_CONTROLLER, "uplink_present");
        public static readonly TelemetryKey DOWNLINK_ACTIVE = new TelemetryKey(Subsystem.S_FLIGHT_CONTROLLER, "downlink_present");

        private static TelemetryKey? getIndex(TelemetryField telemetryField)
        {
            TelemetryKey? key = null;
            foreach (KeyValuePair<TelemetryKey, TelemetryField> kvp in TelemetryFields)
            {
                if (kvp.Value.Code == telemetryField.Code && kvp.Value.Subsystem == telemetryField.Subsystem)
                {
                    key = kvp.Key;
                    break;
                }
            }
            return key;
        }

        private static TelemetryKey register(String code, Semantic semantic, Subsystem subsystem, int subsystemId = 0)
        {
            TelemetryKey key = new TelemetryKey(subsystem, code);
            TelemetryFields.Add(key, TelemetryField.Create(code, semantic, subsystem, subsystemId));
            return key;
        }

        public static TelemetryKey Register(TelemetryField telemetryField)
        {
            TelemetryKey? key = getIndex(telemetryField);
            if (key.HasValue)
                return key.Value; //already present

            return register(telemetryField.Code, telemetryField.Semantic, telemetryField.Subsystem, telemetryField.SubsystemId);
        }
    }
}
