using Avalonia;
using Avalonia.Collections;
using ReactiveUI;
using System.Reactive;
using UGCS.TelemetryViewer.ViewModels.Auxiliary;

namespace UGCS.TelemetryViewer.ViewModels
{
    public interface IMainWindowViewModel
    {

        PixelPoint Position { get; set; }

        double Width { get; set; }

        double Height { get; set; }

        bool UcsConnected { get; }

        bool DroneSelected { get; }

        bool DroneConnected { get; }

        string DroneName { get; }

        ReactiveCommand<Unit, ITelemetryPlate> CreatePlateCommand { get; }

        AvaloniaList<ITelemetryPlate> TelemetryPlates { get; }

    }
}
