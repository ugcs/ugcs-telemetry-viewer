using Avalonia;
using Avalonia.Collections;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using UGCS.UcsServices;

namespace UGCS.TelemetryViewer.ViewModels.Auxiliary
{

    public class TelemetryPlate : ViewModelBase, ITelemetryPlate
    {

        private static readonly IBrush RED_BRUSH = Brush.Parse("#eb4d3d");
        private static readonly IBrush WHITE_BRUSH = Brush.Parse("#f5f9fc");
        private static readonly IBrush GREY_BRUSH = Brush.Parse("#bcc8d2");
        private static readonly IBrush TRANSPARENT_BRUSH = Brush.Parse("#00000000");
        private static readonly IBrush GREY_BACKGROUND_BRUSH = Brush.Parse("#262d33");
        private static readonly IBrush RED_BACKGROUND_BRUSH = Brush.Parse("#282326");

        private TelemetryKey _telemetryKey;
        public string TelemetryKeyCode
        {
            get => _telemetryKey.ToString();
            set
            {
                _telemetryKey = new TelemetryKey(value);
                this.RaisePropertyChanged(nameof(TelemetryKeyCode));
            }
        }

        private string _plateName;
        public string PlateName
        {
            get => _plateName;
            set => this.RaiseAndSetIfChanged(ref _plateName, value);
        }

        private AvaloniaList<Helpers.MenuItem> _menuItems;
        public AvaloniaList<Helpers.MenuItem> MenuItems
        {
            get => _menuItems;
            private set => this.RaiseAndSetIfChanged(ref _menuItems, value);
        }

        private double? _minThreshold;
        public double? MinThreshold
        {
            get => _minThreshold;
            set
            {
                if (value != _minThreshold)
                {
                    _minThreshold = value;
                    this.RaisePropertyChanged(nameof(BackgroundBrush));
                    this.RaisePropertyChanged(nameof(ValueBrush));
                    this.RaisePropertyChanged(nameof(BorderBrush));
                    this.RaisePropertyChanged(nameof(MaxThresholdBrush));
                    this.RaisePropertyChanged(nameof(MinThresholdBrush));
                    this.RaisePropertyChanged(nameof(MinThresholdWeight));
                    this.RaisePropertyChanged(nameof(MaxThresholdWeight));
                    this.RaisePropertyChanged(nameof(MinThreshold));
                    this.RaisePropertyChanged(nameof(MinThresholdText));
                }
            }
        }

        public string MinThresholdText
        {
            get
            {
                if (MinThreshold != null)
                    return $"min {MinThreshold.Value}";
                else
                    return "-";
            }
        }

        private double? _maxThreshold;
        public double? MaxThreshold
        {
            get => _maxThreshold;
            set
            {
                if (value != _maxThreshold)
                {
                    _maxThreshold = value;
                    this.RaisePropertyChanged(nameof(BackgroundBrush));
                    this.RaisePropertyChanged(nameof(ValueBrush));
                    this.RaisePropertyChanged(nameof(BorderBrush));
                    this.RaisePropertyChanged(nameof(MaxThresholdBrush));
                    this.RaisePropertyChanged(nameof(MinThresholdBrush));
                    this.RaisePropertyChanged(nameof(MinThresholdWeight));
                    this.RaisePropertyChanged(nameof(MaxThresholdWeight));
                    this.RaisePropertyChanged(nameof(MaxThreshold));
                    this.RaisePropertyChanged(nameof(MaxThresholdText));
                }
            }
        }

        public string MaxThresholdText
        {
            get
            {
                if (MaxThreshold != null)
                    return $"max {MaxThreshold.Value}";
                else
                    return "-";
            }
        }

        public IBrush MinThresholdBrush
        {
            get
            {
                if (MinThreshold != null && Value < MinThreshold)
                    return RED_BRUSH;
                else
                    return GREY_BRUSH;
            }
        }

        public IBrush MaxThresholdBrush
        {
            get
            {
                if (MaxThreshold != null && Value > MaxThreshold)
                    return RED_BRUSH;
                else
                    return GREY_BRUSH;
            }
        }

        public IBrush BorderBrush
        {
            get
            {
                if ((MaxThreshold != null && Value > MaxThreshold) || (MinThreshold != null && Value < MinThreshold))
                    return RED_BRUSH;
                else
                    return TRANSPARENT_BRUSH;
            }
        }

        public IBrush ValueBrush
        {
            get
            {
                if (Value == null)
                    return GREY_BRUSH;
                if ((MaxThreshold != null && Value > MaxThreshold) || (MinThreshold != null && Value < MinThreshold))
                    return RED_BRUSH;
                return WHITE_BRUSH;
            }
        }

        public IBrush BackgroundBrush
        {
            get
            {
                if ((MaxThreshold != null && Value > MaxThreshold) || (MinThreshold != null && Value < MinThreshold))
                    return RED_BACKGROUND_BRUSH;
                return GREY_BACKGROUND_BRUSH;
            }
        }

        public FontWeight MinThresholdWeight
        {
            get
            {
                if (MaxThreshold != null && Value > MaxThreshold)
                    return FontWeight.Normal;
                return FontWeight.Bold;
            }
        }

        public FontWeight MaxThresholdWeight
        {
            get
            {
                if (MinThreshold != null && Value < MinThreshold)
                    return FontWeight.Normal;
                return FontWeight.Bold;
            }
        }

        private double? _value;

        public double? Value
        {
            get => _value;
            set
            {
                if (value != _value)
                {
                    _value = value;
                    this.RaisePropertyChanged(nameof(BackgroundBrush));
                    this.RaisePropertyChanged(nameof(ValueBrush));
                    this.RaisePropertyChanged(nameof(BorderBrush));
                    this.RaisePropertyChanged(nameof(MaxThresholdBrush));
                    this.RaisePropertyChanged(nameof(MinThresholdBrush));
                    this.RaisePropertyChanged(nameof(MinThresholdWeight));
                    this.RaisePropertyChanged(nameof(MaxThresholdWeight));
                    this.RaisePropertyChanged(nameof(Value));
                    this.RaisePropertyChanged(nameof(ValueText));
                }
            }
        }

        public string ValueText
        {
            get
            {
                if (Value == null)
                {
                    return "N/A";
                }
                else
                {
                    return Math.Round(Value.Value, DecimalPlaces).ToString($"F{DecimalPlaces}");
                }
            }
        }

        private string _units;
        public string Units
        {
            get => _units;
            set => this.RaiseAndSetIfChanged(ref _units, value);
        }

        private ReactiveCommand<Unit, Unit> _backCommand;
        public ReactiveCommand<Unit, Unit> BackCommand
        {
            get => _backCommand;
            set => this.RaiseAndSetIfChanged(ref _backCommand, value);
        }

        private ReactiveCommand<Unit, Unit> _forwardCommand;
        public ReactiveCommand<Unit, Unit> ForwardCommand
        {
            get => _forwardCommand;
            set => this.RaiseAndSetIfChanged(ref _forwardCommand, value);
        }

        public ReactiveCommand<Unit, Unit> RemoveCommand
        {
            get => _remove.OnClickCommand;
            set => _remove.OnClickCommand = value;
        }
        public ReactiveCommand<ITelemetryPlate, Unit> EditCommand { get; set; }

        private int _decimalPlaces;
        public int DecimalPlaces
        {
            get => _decimalPlaces;
            set
            {
                if (_decimalPlaces == value)
                    return;
                _decimalPlaces = value;
                this.RaisePropertyChanged(nameof(DecimalPlaces));
                this.RaisePropertyChanged(nameof(ValueText));
                this.RaisePropertyChanged(nameof(MinThresholdText));
                this.RaisePropertyChanged(nameof(MaxThresholdText));
            }
        }

        private readonly Helpers.MenuItem _remove;

        public TelemetryPlate()
        {
            Helpers.MenuItem editItem = new Helpers.MenuItem()
            {
                Title = "Edit",
                Icon = new Bitmap(AvaloniaLocator.Current.GetService<IAssetLoader>()
                    .Open(new Uri($"avares://telemetry-viewer/Assets/icon-settings-grey.png"))),
                HoveredIcon = new Bitmap(AvaloniaLocator.Current.GetService<IAssetLoader>()
                    .Open(new Uri($"avares://telemetry-viewer/Assets/icon-settings-green.png"))),
                OnClickCommand = ReactiveCommand.CreateFromTask(edit),
            };
            _remove = new Helpers.MenuItem()
            {
                Title = "Remove",
                Icon = new Bitmap(AvaloniaLocator.Current.GetService<IAssetLoader>()
                    .Open(new Uri($"avares://telemetry-viewer/Assets/delete-bin-trash-grey.png"))),
                HoveredIcon = new Bitmap(AvaloniaLocator.Current.GetService<IAssetLoader>()
                    .Open(new Uri($"avares://telemetry-viewer/Assets/delete-bin-trash-green.png"))),
            };
            AvaloniaList<Helpers.MenuItem> menuItems = new AvaloniaList<Helpers.MenuItem>()
            {
                editItem, _remove,
            };
            MenuItems = menuItems;
        }

        private async Task edit()
        {
            if (EditCommand != null)
            {
                await EditCommand.Execute(this).GetAwaiter();
            }
        }
    }
}
