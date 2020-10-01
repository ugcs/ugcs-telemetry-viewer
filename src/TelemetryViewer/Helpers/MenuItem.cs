using Avalonia.Media.Imaging;
using ReactiveUI;
using System.Reactive;

namespace UGCS.TelemetryViewer.Helpers
{

    public class MenuItem
    {
        public ReactiveCommand<Unit, Unit> OnClickCommand { get; set; }
        public IBitmap Icon { get; set; }
        public IBitmap HoveredIcon { get; set; }
        public string Title { get; set; }
    }
}
