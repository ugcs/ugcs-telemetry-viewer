using Com.Ugcs.Ucs.Proto;
using System;
using System.Collections.Generic;

namespace UGCS.UcsServices
{
    public struct TelemetryKey
    {
        public string ComplexCode { get; set; }

        private static string getSubsystemString(Subsystem subsystem)
        {
            switch (subsystem)
            {
                case Subsystem.SControlServer: return "[srv]";
                case Subsystem.SFlightController: return "[f]";
                case Subsystem.SGimbal: return "[g]";
                case Subsystem.SCamera: return "[cam]";
                case Subsystem.SAdsbTransponder: return "[a-t]";
                case Subsystem.SWinch: return "[w]";
                case Subsystem.SHangar: return "[h]";
                case Subsystem.SUser: return "[u]";
                case Subsystem.SGpr: return "[gpr]";
                case Subsystem.SAdsbReceiver: return "[a-r]";
                case Subsystem.SAdsbVehicle: return "[a-v]";
                case Subsystem.SWeatherStation: return "[ws]";
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

        public static readonly TelemetryKey UPLINK_ACTIVE = new TelemetryKey(Subsystem.SFlightController, "uplink_present");
        public static readonly TelemetryKey DOWNLINK_ACTIVE = new TelemetryKey(Subsystem.SFlightController, "downlink_present");

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
