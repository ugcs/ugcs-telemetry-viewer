using Avalonia;
using Avalonia.Collections;
using DynamicData.Binding;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using UGCS.TelemetryViewer.Helpers;
using UGCS.TelemetryViewer.Models;
using UGCS.TelemetryViewer.Services;
using UGCS.TelemetryViewer.ViewModels.Auxiliary;
using UGCS.TelemetryViewer.ViewModels.Intrerfaces;
using UGCS.TelemetryViewer.Views;
using UGCS.UcsServices;
using UGCS.UcsServices.DTO;

namespace UGCS.TelemetryViewer.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, IMainWindowViewModel
    {
        private readonly ISelectedVehicleContainer _selectedVehicleContainer;
        private readonly IAppContextContainer _appContextContainer;

        private IClientVehicle _previousVehicle;

        private bool _ucsConnected = true;
        public bool UcsConnected
        {
            get => _ucsConnected;
            private set => this.RaiseAndSetIfChanged(ref _ucsConnected, value);
        }

        public bool DroneSelected => _selectedVehicleContainer.SelectedVehicle != null;

        private readonly Dictionary<int, AvaloniaList<ITelemetryPlate>> _dronePlatesMap;

        private AvaloniaList<ITelemetryPlate> _telemetryPlates;
        public AvaloniaList<ITelemetryPlate> TelemetryPlates
        {
            get => _telemetryPlates;
            private set => this.RaiseAndSetIfChanged(ref _telemetryPlates, value);
        }

        public string DroneName
        {
            get
            {
                if (_selectedVehicleContainer.SelectedVehicle == null)
                    return "-";
                else
                    return _selectedVehicleContainer.SelectedVehicle.Name;
            }
        }

        public ReactiveCommand<Unit, ITelemetryPlate> CreatePlateCommand { get; private set; }

        private PixelPoint _position;
        public PixelPoint Position
        {
            get => _position;
            set => this.RaiseAndSetIfChanged(ref _position, value);
        }

        private double _width;
        public double Width
        {
            get => _width;
            set => this.RaiseAndSetIfChanged(ref _width, value);
        }

        private double _height;
        public double Height
        {
            get => _height;
            set => this.RaiseAndSetIfChanged(ref _height, value);
        }

        private bool _droneConnected;
        public bool DroneConnected
        {
            get => _droneConnected;
            set => this.RaiseAndSetIfChanged(ref _droneConnected, value);
        }

        public MainWindowViewModel(ConnectionService connectionService, ISelectedVehicleContainer selectedVehicleContainer,
            IAppContextContainer appContextContainer)
        {
            _appContextContainer = appContextContainer ?? throw new ArgumentNullException(nameof(appContextContainer));
            _selectedVehicleContainer = selectedVehicleContainer ?? throw new ArgumentNullException(nameof(selectedVehicleContainer));
            if (connectionService == null)
                throw new ArgumentNullException(nameof(connectionService));

            initVehicleTelemetryMonitoring();

            _dronePlatesMap = _appContextContainer.Context.vehiclePlatesMap;
            onDroneChanged(null, null);
            foreach (var kvp in _dronePlatesMap)
            {
                foreach (ITelemetryPlate plate in kvp.Value)
                {
                    setCommandsToPlate(plate);
                }
                // ******** Why is this needed *********
                // Because all comands CanExecute observables are binded on
                // list change, so they are not receive signal that them can
                // be executed until list is not change. This fake change is
                // enough to trigger it.
                if (kvp.Value.Count > 0)
                {
                    kvp.Value.Move(0, 0);
                }
                // *************************************
            }
            Width = _appContextContainer.Context.mainWindowWidth;
            Height = _appContextContainer.Context.mainWindowHeight;
            Position = _appContextContainer.Context.mainWindowPosition;
            this.WhenAnyValue(o => o.Width).Subscribe(d =>
            {
                _appContextContainer.Context.mainWindowWidth = d;
            });
            this.WhenAnyValue(o => o.Height).Subscribe(d =>
            {
                _appContextContainer.Context.mainWindowHeight = d;
            });
            this.WhenAnyValue(o => o.Position).Subscribe(p =>
            {
                _appContextContainer.Context.mainWindowPosition = p;
            });

            CreatePlateCommand = ReactiveCommand.CreateFromTask(createTelemetryPlate);

            connectionService.Disconnected += onDisconnect;
            connectionService.Connected += onConnected;

        }

        private async Task<Unit> editTelemetryPlate(ITelemetryPlate sourcePlate)
        {
            CreatePlateWindow dialog = TelemetryViewerApp.Current.ViewFactory
                .CreateWindow<CreatePlateWindow>();
            ITelemetryPlate copyPlate = ((ICreatePlateWindowViewModel)dialog.DataContext).TelemetryPlate;
            copyPlate.PlateName = sourcePlate.PlateName;
            copyPlate.TelemetryKeyCode = sourcePlate.TelemetryKeyCode;
            copyPlate.Units = sourcePlate.Units;
            copyPlate.MinThreshold = sourcePlate.MinThreshold;
            copyPlate.MaxThreshold = sourcePlate.MaxThreshold;
            copyPlate.DecimalPlaces = sourcePlate.DecimalPlaces;
            ITelemetryPlate plate = await dialog.ShowDialog<ITelemetryPlate>(TelemetryViewerApp.Current.Desktop.MainWindow);
            if (plate != null)
            {
                sourcePlate.PlateName = plate.PlateName;
                sourcePlate.TelemetryKeyCode = plate.TelemetryKeyCode;
                sourcePlate.Units = plate.Units;
                sourcePlate.MinThreshold = plate.MinThreshold;
                sourcePlate.MaxThreshold = plate.MaxThreshold;
                sourcePlate.DecimalPlaces = plate.DecimalPlaces;
                TelemetryKey key = new TelemetryKey(sourcePlate.TelemetryKeyCode);
                if (_selectedVehicleContainer.SelectedVehicle.Telemetry.TryGetValue(key, out TelemetryValue value))
                {
                    var rawValue = TelemetryValueToDoubleConverter.Convert(value);
                    sourcePlate.Value = TelemetryUnitConverter.Convert(key, rawValue);
                }
            }
            return new Unit();
        }

        private async Task<ITelemetryPlate> createTelemetryPlate()
        {
            CreatePlateWindow dialog = TelemetryViewerApp.Current.ViewFactory
                .CreateWindow<CreatePlateWindow>();
            ITelemetryPlate plate = await dialog.ShowDialog<ITelemetryPlate>(TelemetryViewerApp.Current.Desktop.MainWindow);
            if (plate != null)
            {
                setCommandsToPlate(plate);
                if (_selectedVehicleContainer.SelectedVehicle.Telemetry.TryGetValue(new TelemetryKey(plate.TelemetryKeyCode), out TelemetryValue value))
                {
                    var rawValue = TelemetryValueToDoubleConverter.Convert(value);
                    plate.Value = TelemetryUnitConverter.Convert(new TelemetryKey(plate.TelemetryKeyCode), rawValue);
                }
            }
            return plate;
        }

        private void setCommandsToPlate(ITelemetryPlate plate)
        {
            plate.RemoveCommand = ReactiveCommand.Create(() => { _telemetryPlates.Remove(plate); });
            plate.EditCommand = ReactiveCommand.CreateFromTask<ITelemetryPlate, Unit>(editTelemetryPlate);

            plate.BackCommand = ReactiveCommand.Create(() =>
                {
                    int index = TelemetryPlates.IndexOf(plate);
                    TelemetryPlates.Move(index, index - 1);
                },
                TelemetryPlates.ObserveCollectionChanges()
                .Select(e =>
                {
                    var alist = (AvaloniaList<ITelemetryPlate>)e.Sender;
                    return alist.IndexOf(plate) > -1;
                }));

            plate.ForwardCommand = ReactiveCommand.Create(() =>
                {
                    int index = TelemetryPlates.IndexOf(plate);
                    TelemetryPlates.Move(index, index + 1);
                },
                TelemetryPlates.ObserveCollectionChanges()
                .Select(e =>
                {
                    var alist = (AvaloniaList<ITelemetryPlate>)e.Sender;
                    return alist.IndexOf(plate) < alist.Count - 1;
                }));
        }

        private void initVehicleTelemetryMonitoring()
        {
            _selectedVehicleContainer.OnSelectedVehicleChanged += onDroneChanged;
            _selectedVehicleContainer.OnNewTelemetryReceived += onTelemetryChanged;
        }

        private void onDisconnect(object sender, EventArgs args)
        {
            UcsConnected = false;
        }

        private void onConnected(object sender, EventArgs args)
        {
            initVehicleTelemetryMonitoring();
            UcsConnected = true;
        }

        private void onDroneChanged(object sender, EventArgs args)
        {
            this.RaisePropertyChanged(nameof(DroneName));
            this.RaisePropertyChanged(nameof(DroneSelected));
            this.RaisePropertyChanged(nameof(DroneConnected));
            if (_previousVehicle != null)
                _previousVehicle.IsConnectChange -= onIsConnectChange;
            _previousVehicle = _selectedVehicleContainer.SelectedVehicle;

            if (_selectedVehicleContainer.SelectedVehicle != null)
            {
                _selectedVehicleContainer.SelectedVehicle.IsConnectChange += onIsConnectChange;
                DroneConnected = _selectedVehicleContainer.SelectedVehicle.IsConnected;
                showPlatesForVehicleById(_selectedVehicleContainer.SelectedVehicle.Id);
            }
            else
            {
                DroneConnected = false;
                hidePlates();
            }
        }

        private void onIsConnectChange(object sender, bool value)
        {
            DroneConnected = value;
        }

        private void hidePlates()
        {
            TelemetryPlates = null;
        }

        private void showPlatesForVehicleById(int vehicleId)
        {
            if (!_dronePlatesMap.ContainsKey(vehicleId))
            {
                _dronePlatesMap.Add(vehicleId, new AvaloniaList<ITelemetryPlate>());
            }
            TelemetryPlates = _dronePlatesMap[vehicleId];
        }

        private AvaloniaList<ITelemetryPlate> getPlatesForVehicleById(int vehicleId)
        {
            if (!_dronePlatesMap.ContainsKey(vehicleId))
            {
                _dronePlatesMap.Add(vehicleId, new AvaloniaList<ITelemetryPlate>());
            }
            return _dronePlatesMap[vehicleId];
        }

        private void onTelemetryChanged(object sender, TelemetryData data)
        {
            AvaloniaList<ITelemetryPlate> plates = getPlatesForVehicleById(data.VehicleId);
            foreach (ITelemetryPlate plate in plates)
            {
                if (plate.TelemetryKeyCode == data.TelemetryKey.ComplexCode)
                {
                    var rawValue = TelemetryValueToDoubleConverter.Convert(data.TelemetryValue);
                    plate.Value = TelemetryUnitConverter.Convert(new TelemetryKey(plate.TelemetryKeyCode), rawValue);
                }
            }
        }

    }
}
