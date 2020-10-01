using System;
using UGCS.TelemetryViewer.Helpers;
using UGCS.TelemetryViewer.Models;

namespace UGCS.TelemetryViewer
{
    public interface ISelectedVehicleContainer
    {

        event EventHandler<TelemetryData> OnNewTelemetryReceived;

        IClientVehicle SelectedVehicle { get; }

        event EventHandler OnSelectedVehicleChanged;

    }
}
