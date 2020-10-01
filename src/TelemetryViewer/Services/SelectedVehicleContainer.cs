using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UGCS.TelemetryViewer.Helpers;
using UGCS.TelemetryViewer.Models;
using UGCS.UcsServices;
using UGCS.UcsServices.DTO;

namespace UGCS.TelemetryViewer.Services
{
    public class SelectedVehicleContainer : ISelectedVehicleContainer
    {
        /// <summary>
        /// Represent vehicle in view.    
        /// </summary>
        private class ClientVehicle : IClientVehicle
        {
            public int Id { get; set; }
            public string Name { get; set; }
            private bool _isConnect;
            public bool IsConnected
            {
                get => _isConnect;
                set
                {
                    _isConnect = value;
                    IsConnectChange?.Invoke(this, value);
                }
            }
            public ConcurrentDictionary<TelemetryKey, TelemetryValue> Telemetry { get; set; } = new ConcurrentDictionary<TelemetryKey, TelemetryValue>();

            public event EventHandler<bool> IsConnectChange;
        }

        private static readonly ILog _log = LogManager.GetLogger(typeof(SelectedVehicleContainer));

        private ClientVehicle _selectedVehicle;
        public IClientVehicle SelectedVehicle
        {
            get
            {
                return _selectedVehicle;
            }
            set
            {
                _selectedVehicle = (ClientVehicle)value;
                fetchExistingTelemetry();
                OnSelectedVehicleChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler<TelemetryData> OnNewTelemetryReceived;
        public event EventHandler OnSelectedVehicleChanged;

        private readonly TelemetryListener _telemetryListener;

        public SelectedVehicleContainer(TelemetryListener telemetryListener, VehicleService vehicleService)
        {
            _telemetryListener = telemetryListener ?? throw new ArgumentNullException(nameof(telemetryListener));
            if (vehicleService == null)
                throw new ArgumentNullException(nameof(vehicleService));

            var vehicle = vehicleService.GetSelectedVehicleId();
            if (vehicle != null)
            {
                SelectedVehicle = createClientVehicleWithTelemetry(vehicle);
            }
            vehicleService.SubscribeSelectedVehicleChange((vehicle) =>
            {
                SelectedVehicle = createClientVehicleWithTelemetry(vehicle);
            });

            _telemetryListener.SubscribeTelemtry(telemetryValueChanged);
        }

        private void telemetryValueChanged(int vehicleId, TelemetryKey tk, TelemetryValue tv)
        {
            if (SelectedVehicle != null && SelectedVehicle.Id == vehicleId)
            {
                setTelemetry(tk, tv);
            }
        }

        private ClientVehicle createClientVehicleWithTelemetry(ClientVehicleDto clientVehicleDTO)
        {
            if (clientVehicleDTO == null)
            {
                return null;
            }
            ClientVehicle clientVehicle = new ClientVehicle()
            {
                Id = clientVehicleDTO.VehicleId,
                Name = clientVehicleDTO.Name,
            };
            return clientVehicle;
        }

        private void fetchExistingTelemetry()
        {
            if (SelectedVehicle == null)
                return;

            foreach (KeyValuePair<TelemetryKey, TelemetryValue> val in _telemetryListener.GetTelemetryById(SelectedVehicle.Id))
            {
                setTelemetry(val.Key, val.Value);
            }
        }

        private void setTelemetry(TelemetryKey tk, TelemetryValue tv)
        {
            if (!SelectedVehicle.Telemetry.ContainsKey(tk))
            {
                _log.Info($"New telemetry key received. vehicleId={SelectedVehicle.Id} vehicleName={SelectedVehicle.Name} key={tk.ComplexCode}");
                SelectedVehicle.Telemetry.AddOrUpdate(tk, tv, (k, v) => tv);
            }
            else if (tv != null)
                SelectedVehicle.Telemetry.AddOrUpdate(tk, tv, (k, v) => tv);
            if (tk.Equals(TelemetryKeys.UPLINK_ACTIVE) || tk.Equals(TelemetryKeys.DOWNLINK_ACTIVE))
            {
                if (tv != null)
                    _selectedVehicle.IsConnected = true;
                else
                    _selectedVehicle.IsConnected = false;
            }
            OnNewTelemetryReceived?.Invoke(this, new TelemetryData() { TelemetryKey = tk, TelemetryValue = tv, VehicleId = SelectedVehicle.Id });
        }
    }
}
