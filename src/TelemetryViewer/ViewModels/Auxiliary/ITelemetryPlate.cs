using Avalonia.Collections;
using Avalonia.Media;
using ReactiveUI;
using System.Reactive;
using UGCS.TelemetryViewer.Helpers;

namespace UGCS.TelemetryViewer.ViewModels.Auxiliary
{
    public interface ITelemetryPlate
    {
        ReactiveCommand<Unit, Unit> BackCommand { get; set; }
        IBrush BackgroundBrush { get; }
        IBrush BorderBrush { get; }
        ReactiveCommand<Unit, Unit> ForwardCommand { get; set; }
        string MaxThresholdText { get; }
        double? MaxThreshold { get; set; }
        IBrush MaxThresholdBrush { get; }
        FontWeight MaxThresholdWeight { get; }
        string MinThresholdText { get; }
        string Units { get; set; }
        double? MinThreshold { get; set; }
        IBrush MinThresholdBrush { get; }
        FontWeight MinThresholdWeight { get; }
        string PlateName { get; set; }
        ReactiveCommand<Unit, Unit> RemoveCommand { get; set; }
        ReactiveCommand<ITelemetryPlate, Unit> EditCommand { get; set; }
        string TelemetryKeyCode { get; set; }
        double? Value { get; set; }
        IBrush ValueBrush { get; }
        string ValueText { get; }
        int DecimalPlaces { get; set; }
        AvaloniaList<MenuItem> MenuItems { get; }
    }
}