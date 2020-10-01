using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using System;
using UGCS.TelemetryViewer.Helpers;
using UGCS.TelemetryViewer.ViewModels.Auxiliary;

namespace UGCS.TelemetryViewer.Views
{
    public class TelemetryPlateSettings : UserControl
    {

        public static readonly DirectProperty<TelemetryPlateSettings, AvaloniaList<string>> TelemetryVariantsProperty =
            AvaloniaProperty.RegisterDirect<TelemetryPlateSettings, AvaloniaList<string>>(nameof(TelemetryVariants),
                o => o.TelemetryVariants,
                (o, v) => o.TelemetryVariants = v,
                defaultBindingMode: BindingMode.TwoWay);

        public AvaloniaList<string> TelemetryVariants
        {
            get => _telemetryBox.Variants;
            set => _telemetryBox.Variants = value;
        }

        public static readonly DirectProperty<TelemetryPlateSettings, bool> IsValidProperty =
            AvaloniaProperty.RegisterDirect<TelemetryPlateSettings, bool>(nameof(IsValid),
                o => o.IsValid,
                (o, v) => o.IsValid = v,
                defaultBindingMode: BindingMode.TwoWay);

        private bool _isValid;
        public bool IsValid
        {
            get => _isValid;
            set
            {
                if (_isValid == value)
                    return;
                bool past = _isValid;
                _isValid = value;
                this.RaisePropertyChanged(IsValidProperty, past, _isValid);
            }
        }

        public static readonly DirectProperty<TelemetryPlateSettings, ITelemetryPlate> EditingPlateProperty =
            AvaloniaProperty.RegisterDirect<TelemetryPlateSettings, ITelemetryPlate>(nameof(EditingPlate),
                o => o.EditingPlate,
                (o, v) => o.EditingPlate = v,
                defaultBindingMode: BindingMode.TwoWay);

        private ITelemetryPlate _editingPlate;
        public ITelemetryPlate EditingPlate
        {
            get => _editingPlate;
            set
            {
                _editingPlate = value;
                updateBindings();
            }
        }

        private readonly StyledTextBox _minThresholdBox;
        private readonly StyledTextBox _maxThresholdBox;
        private readonly StyledTextBox _decimalPlacesBox;
        private readonly StyledSelectTextBox _telemetryBox;
        private readonly StyledTextBox _captionBox;
        private readonly StyledTextBox _unitsBox;

        public TelemetryPlateSettings()
        {
            this.initializeComponent();
            _telemetryBox = this.FindControl<StyledSelectTextBox>("TelemSelect");
            _minThresholdBox = this.FindControl<StyledTextBox>("MinThresholdBox");
            _maxThresholdBox = this.FindControl<StyledTextBox>("MaxThresholdBox");
            _decimalPlacesBox = this.FindControl<StyledTextBox>("DecimalPlacesBox");
            _captionBox = this.FindControl<StyledTextBox>("CaptionBox");
            _unitsBox = this.FindControl<StyledTextBox>("UnitsBox");

            _captionBox.ValidateCommand = ReactiveCommand.Create<string, bool>(notEmptyValidation);
            _telemetryBox.ValidateCommand = ReactiveCommand.Create<string, bool>(notEmptyValidation);
            _minThresholdBox.ValidateCommand = ReactiveCommand.Create<string, bool>(numberValidation);
            _maxThresholdBox.ValidateCommand = ReactiveCommand.Create<string, bool>(numberValidation);
            _decimalPlacesBox.ValidateCommand = ReactiveCommand.Create<string, bool>(numberRangeValidation);

            _captionBox.WhenAnyValue(o => o.Valid).Subscribe((s) => updateValid());
            _telemetryBox.WhenAnyValue(o => o.Valid).Subscribe((s) => updateValid());
            _minThresholdBox.WhenAnyValue(o => o.Valid).Subscribe((s) => updateValid());
            _maxThresholdBox.WhenAnyValue(o => o.Valid).Subscribe((s) => updateValid());
            _decimalPlacesBox.WhenAnyValue(o => o.Valid).Subscribe((s) => updateValid());
        }

        private void updateValid()
        {
            IsValid = _captionBox.Valid
                && _telemetryBox.Valid
                && _minThresholdBox.Valid
                && _maxThresholdBox.Valid
                && _decimalPlacesBox.Valid;
        }

        private bool notEmptyValidation(string text) => !string.IsNullOrEmpty(text);

        private bool numberValidation(string text) =>
            string.IsNullOrEmpty(text) || double.TryParse(text, out double _);

        private bool numberRangeValidation(string text) =>
            string.IsNullOrEmpty(text) || (int.TryParse(text, out int i) && i < 8 && i >= 0);

        private void initializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void updateBindings()
        {
            if (EditingPlate != null)
            {

                // Caption
                _captionBox.Bind<string>(StyledTextBox.TextProperty, EditingPlate
                    .WhenAnyValue(
                    o => o.PlateName));
                _captionBox.GetObservable(StyledTextBox.TextProperty).Subscribe(s => EditingPlate.PlateName = s);

                // TelemetryKeyCode
                _telemetryBox.Bind<string>(StyledSelectTextBox.TextProperty, EditingPlate
                    .WhenAnyValue(
                    o => o.TelemetryKeyCode));
                _telemetryBox.GetObservable(StyledSelectTextBox.TextProperty).Subscribe(s => EditingPlate.TelemetryKeyCode = s);

                // Units
                _unitsBox.Bind<string>(StyledTextBox.TextProperty, EditingPlate
                    .WhenAnyValue(
                    o => o.Units));
                _unitsBox.GetObservable(StyledTextBox.TextProperty).Subscribe(s => EditingPlate.Units = s);

                // Decimal places
                _decimalPlacesBox.Bind<string>(StyledTextBox.TextProperty, EditingPlate
                    .WhenAny(
                    o => o.DecimalPlaces,
                    o =>
                    {
                        return StringToIntConverter.Convert(o.Value);
                    }));
                _decimalPlacesBox.GetObservable(StyledTextBox.TextProperty).Subscribe(s => EditingPlate.DecimalPlaces = StringToIntConverter.ConvertBack(s) ?? 0);

                // Min
                _minThresholdBox.Bind<string>(StyledTextBox.TextProperty, EditingPlate
                    .WhenAny(
                    o => o.MinThreshold,
                    o =>
                    {
                        return StringDoubleConverter.Convert(o.Value);
                    }));
                _minThresholdBox.GetObservable(StyledTextBox.TextProperty).Subscribe(s => EditingPlate.MinThreshold = StringDoubleConverter.ConvertBack(s));

                // Max
                _maxThresholdBox.Bind<string>(StyledTextBox.TextProperty, EditingPlate
                    .WhenAny(
                    o => o.MaxThreshold,
                    o =>
                    {
                        return StringDoubleConverter.Convert(o.Value);
                    }));
                _maxThresholdBox.GetObservable(StyledTextBox.TextProperty).Subscribe(s => EditingPlate.MaxThreshold = StringDoubleConverter.ConvertBack(s));
            }
        }
    }
}
