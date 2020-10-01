using System;
using System.Collections.Generic;
using UGCS.Sdk.Protocol;
using UGCS.Sdk.Protocol.Encoding;
using UGCS.UcsServices.DTO;

namespace UGCS.UcsServices
{
    public class TelemetryListener
    {
        private static readonly Dictionary<int, Dictionary<TelemetryKey, TelemetryValue>> _latestValues = new Dictionary<int, Dictionary<TelemetryKey, TelemetryValue>>(); //by vehicle Id
        private const int POLLING_INTERVAL = 100;
        public delegate void TelemetryBatchSubscriptionCallback(List<VehicleTelemetry> telemetry);
        private readonly ConnectionService _connectionService;
        private readonly EventSubscriptionWrapper _eventSubscriptionWrapper;
        private Action<int, TelemetryKey, TelemetryValue> _tlmCallBack;

        public TelemetryListener(ConnectionService connect)
        {
            _connectionService = connect;
            _eventSubscriptionWrapper = new EventSubscriptionWrapper();
        }
        public void SubscribeTelemtry(Action<int, TelemetryKey, TelemetryValue> cb)
        {
            _tlmCallBack = cb;
            _eventSubscriptionWrapper.TelemetryBatchSubscription = new TelemetryBatchSubscription()
            {
                PollingPeriodMilliseconds = POLLING_INTERVAL,
                PollingPeriodMillisecondsSpecified = true
            };

            SubscribeEventRequest requestEvent = new SubscribeEventRequest
            {
                ClientId = _connectionService.GetClientId(),

                Subscription = _eventSubscriptionWrapper
            };
            var responce = _connectionService.Submit<SubscribeEventResponse>(requestEvent);
            if (responce.Exception != null)
            {
                throw responce.Exception;
            }
            if (responce.Value == null)
            {
                throw new InvalidOperationException("Server return empty response on SubscribeTelemtry event");
            }
            var subscribeEventResponse = responce.Value;

            SubscriptionToken st = new SubscriptionToken(subscribeEventResponse.SubscriptionId, getTelemetryNotificationHandler(
                (telemetry) =>
                {
                    onTelemetryBatchReceived(telemetry);
                }), _eventSubscriptionWrapper);
            _connectionService.NotificationListener.AddSubscription(st);

        }

        private NotificationHandler getTelemetryNotificationHandler(TelemetryBatchSubscriptionCallback callback)
        {
            return notification =>
            {
                TelemetryBatchEvent @event = notification.Event.TelemetryBatchEvent;
                callback(@event.VehicleTelemetry);
            };
        }

        public Dictionary<TelemetryKey, TelemetryValue> GetTelemetryById(int vehicleId)
        {
            lock (_latestValues)
            {
                if (!_latestValues.ContainsKey(vehicleId))
                {
                    return new Dictionary<TelemetryKey, TelemetryValue>();
                }
                return _latestValues[vehicleId];
            }
        }

        /// <summary>
        /// Telemetry fill
        /// </summary>
        /// <param name="vehicleId">vehicle id</param>
        /// <param name="telemetry">list with telemetry values telemetry</param>
        private void onTelemetryBatchReceived(List<VehicleTelemetry> listOfTelemetry)
        {
            for (int k = 0; k < listOfTelemetry.Count; k++)
            {
                onTelemetryReceived(listOfTelemetry[k]);
            }
        }

        public static T? GetValueOrNull<T>(Value telemetryValue) where T : struct
        {
            if (telemetryValue == null) return null;

            T? returnValue = null;

            if (typeof(T) == typeof(float))
            {
                returnValue = (T)Convert.ChangeType(telemetryValue.FloatValue, typeof(T));
            }
            if (typeof(T) == typeof(long))
            {
                returnValue = (T)Convert.ChangeType(telemetryValue.LongValue, typeof(T));
            }
            if (typeof(T) == typeof(int))
            {
                returnValue = (T)Convert.ChangeType(telemetryValue.IntValue, typeof(T));
            }
            if (typeof(T) == typeof(byte))
            {
                returnValue = (T)Convert.ChangeType(telemetryValue.IntValue, typeof(T));
            }
            if (typeof(T) == typeof(bool))
            {
                returnValue = (T)Convert.ChangeType(telemetryValue.BoolValue, typeof(T));
            }
            if (typeof(T) == typeof(double))
            {
                returnValue = (T)Convert.ChangeType(telemetryValue.DoubleValue, typeof(T));
            }

            return returnValue;
        }

        public static T GetValueOrDefault<T>(Value telemetryValue) where T : struct
        {
            return GetValueOrNull<T>(telemetryValue).GetValueOrDefault();
        }
        private void onTelemetryReceived(VehicleTelemetry listOfTelemetry)
        {
            int vehicleId = listOfTelemetry.Vehicle.Id;
            Dictionary<TelemetryKey, TelemetryValue> dict = new Dictionary<TelemetryKey, TelemetryValue>();

            lock (_latestValues)
            {
                if (!_latestValues.TryGetValue(vehicleId, out _))
                {
                    _latestValues.Add(vehicleId, new Dictionary<TelemetryKey, TelemetryValue>());
                }
            }
            lock (_latestValues)
            {
                int count = listOfTelemetry.Telemetry.Count;
                for (int i = 0; i < count; i++)
                {
                    Telemetry telemetry = listOfTelemetry.Telemetry[i];
                    if (telemetry.TelemetryField == null)
                        continue;

                    TelemetryKey key = TelemetryKeys.Register(telemetry.TelemetryField);
                    var tv = TelemetryValue.MapValue(telemetry.Value);
                    if (!_latestValues[vehicleId].ContainsKey(key))
                    {
                        _latestValues[vehicleId].Add(key, tv);
                    }
                    else
                    {
                        _latestValues[vehicleId][key] = tv;
                    }
                    if (!dict.ContainsKey(key))
                    {
                        dict.Add(key, tv);
                    }
                    else
                    {
                        dict[key] = tv;
                    }
                }
            }
            if (_tlmCallBack != null)
            {
                foreach (KeyValuePair<TelemetryKey, TelemetryValue> entry in dict)
                {
                    _tlmCallBack(vehicleId, entry.Key, entry.Value);
                }
            }
        }

    }
}
