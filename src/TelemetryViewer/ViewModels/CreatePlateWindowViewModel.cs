using Avalonia.Collections;
using ReactiveUI;
using System;
using System.Linq;
using UGCS.Sdk.Protocol.Encoding;
using UGCS.TelemetryViewer.Helpers;
using UGCS.TelemetryViewer.ViewModels.Auxiliary;
using UGCS.TelemetryViewer.ViewModels.Intrerfaces;
using UGCS.UcsServices;

namespace UGCS.TelemetryViewer.ViewModels
{
    public class CreatePlateWindowViewModel : ViewModelBase, ICreatePlateWindowViewModel
    {

        private static readonly Semantic[] NON_NUMERIC_SEMANTIC =
            {
            Semantic.S_GPS_FIX_TYPE,
            Semantic.S_CONTROL_MODE,
            Semantic.S_ADSB_MODE,
            Semantic.S_BOOL,
            Semantic.S_STRING,
            Semantic.S_ENUM,
            Semantic.S_FLIGHT_MODE,
            Semantic.S_LIST,
            Semantic.S_AUTOPILOT_STATUS,
            Semantic.S_ICAO,
            Semantic.S_SQUAWK,
            Semantic.S_ANY,
            };

        private bool _correctInput;
        public bool CorrectInput
        {
            get => _correctInput;
            set => this.RaiseAndSetIfChanged(ref _correctInput, value);
        }

        private ITelemetryPlate _telemetryPlate;
        public ITelemetryPlate TelemetryPlate
        {
            get => _telemetryPlate;
            private set => this.RaiseAndSetIfChanged(ref _telemetryPlate, value);
        }

        private AvaloniaList<string> _telemetryVariants;
        public AvaloniaList<string> TelemetryVariants
        {
            get => _telemetryVariants;
            private set => this.RaiseAndSetIfChanged(ref _telemetryVariants, value);
        }

        private readonly ISelectedVehicleContainer _selectedVehicleContainer;

        public CreatePlateWindowViewModel(ISelectedVehicleContainer selectedVehicleContainer,
            ITelemetryPlateFactory telemetryPlateFactory)
        {
            if (telemetryPlateFactory == null)
                throw new ArgumentNullException(nameof(telemetryPlateFactory));
            _selectedVehicleContainer = selectedVehicleContainer ?? throw new ArgumentNullException(nameof(selectedVehicleContainer));
            if (_selectedVehicleContainer.SelectedVehicle != null)
            {
                TelemetryVariants = getTelemetry();
            }
            _selectedVehicleContainer.OnSelectedVehicleChanged += onDroneChanged;
            _selectedVehicleContainer.OnNewTelemetryReceived += onTelemetryReceived;
            TelemetryPlate = telemetryPlateFactory.Create(String.Empty, String.Empty, String.Empty, null, null, 0);
        }

        private void onDroneChanged(object sender, EventArgs e)
        {
            if (_selectedVehicleContainer.SelectedVehicle != null)
            {
                TelemetryVariants = getTelemetry();
            }
        }

        private AvaloniaList<string> getTelemetry()
        {
            return new AvaloniaList<string>(
                   _selectedVehicleContainer.SelectedVehicle.Telemetry
                   .Where(p => !NON_NUMERIC_SEMANTIC.Contains(TelemetryKeys.GetTelemetryFieldByKey(p.Key).Semantic))
                   .Select(p => p.Key.ComplexCode).OrderBy(s => s));
        }

        private void onTelemetryReceived(object sender, TelemetryData telemetry)
        {
            if (!TelemetryVariants.Contains(telemetry.TelemetryKey.ComplexCode) && telemetry.TelemetryKey.ComplexCode != null)
            { 
                TelemetryVariants.Add(telemetry.TelemetryKey.ComplexCode);
                TelemetryVariants = new AvaloniaList<string>(TelemetryVariants.OrderBy(s => s));
            }
        }
    }
}
