using Avalonia;
using Avalonia.Collections;
using System.Collections.Generic;
using UGCS.TelemetryViewer.ViewModels.Auxiliary;

namespace UGCS.TelemetryViewer.Helpers
{

    public class AppContext
    {
        public Dictionary<int, AvaloniaList<ITelemetryPlate>> vehiclePlatesMap { get; set; }

        public double mainWindowWidth { get; set; }

        public double mainWindowHeight { get; set; }

        public PixelPoint mainWindowPosition { get; set; }
    }
}
