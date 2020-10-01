using Avalonia.Collections;
using UGCS.TelemetryViewer.ViewModels.Auxiliary;

namespace UGCS.TelemetryViewer.ViewModels.Intrerfaces
{
    public interface ICreatePlateWindowViewModel
    {

        AvaloniaList<string> TelemetryVariants { get; }

        bool CorrectInput { get; set; }

        ITelemetryPlate TelemetryPlate { get; }

    }
}
