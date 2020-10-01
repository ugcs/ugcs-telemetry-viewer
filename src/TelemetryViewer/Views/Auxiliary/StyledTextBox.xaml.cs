using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using ReactiveUI;
using System;

namespace UGCS.TelemetryViewer.Views
{
    public class StyledTextBox : UserControl
    {

        private const string DEFAULT_CONTOUR_COLOR = "#4b5762";
        private const string DEFAULT_INVALID_CONTOUR_COLOR = "#ff0000";

        public static readonly StyledProperty<double> ContentFontSizeProperty =
            AvaloniaProperty.Register<StyledTextBox, double>(nameof(ContentFontSize), 12, true);

        public double ContentFontSize
        {
            get
            {
                return GetValue(ContentFontSizeProperty);
            }
            set
            {
                SetValue(ContentFontSizeProperty, value);
                _tooltip.FontSize = ContentFontSize;
                _contentBox.FontSize = ContentFontSize;
            }
        }

        public static readonly StyledProperty<double> TooltipFontSizeProperty =
            AvaloniaProperty.Register<StyledTextBox, double>(nameof(TooltipFontSize), 12, true);

        public double TooltipFontSize
        {
            get
            {
                return GetValue(TooltipFontSizeProperty);
            }
            set
            {
                SetValue(TooltipFontSizeProperty, value);
                _topTooltip.FontSize = TooltipFontSize;
            }
        }

        public static readonly StyledProperty<string> TooltipTextProperty =
            AvaloniaProperty.Register<StyledTextBox, string>(nameof(TooltipText), "Tooltip", true);

        public string TooltipText
        {
            get
            {
                return GetValue(TooltipTextProperty);
            }
            set
            {
                SetValue(TooltipTextProperty, value);
                _tooltip.Text = TooltipText;
                _topTooltip.Text = TooltipText;
            }
        }

        public static readonly DirectProperty<StyledTextBox, string> TextProperty =
            AvaloniaProperty.RegisterDirect<StyledTextBox, string>(nameof(Text),
                o => o.Text,
                (o, v) => o.Text = v,
                defaultBindingMode: BindingMode.TwoWay);

        private string _text;
        public string Text
        {
            get => _text;
            set
            {
                if (value == _text)
                    return;
                string past = _text;
                _text = value;
                _contentBox.Text = value;
                OnTextChange(_text);
                RaisePropertyChanged(TextProperty, past, _text);
                _validateCommand?.Execute(_text).Subscribe(b =>
                {
                    Valid = b;
                });
            }
        }

        public static readonly StyledProperty<IBitmap> ImageProperty =
            AvaloniaProperty.Register<StyledTextBox, IBitmap>(nameof(Image), null, true);

        public IBitmap Image
        {
            get
            {
                return GetValue(ImageProperty);
            }
            set
            {
                SetValue(ImageProperty, value);
                _imageView.Source = Image;
            }
        }

        public static readonly StyledProperty<IBrush> TooltipForegroundProperty =
            AvaloniaProperty.Register<StyledTextBox, IBrush>(nameof(TooltipForeground), null, true);

        public IBrush TooltipForeground
        {
            get
            {
                return GetValue(TooltipForegroundProperty);
            }
            set
            {
                SetValue(TooltipForegroundProperty, value);
                _tooltip.Foreground = TooltipForeground;
                _topTooltip.Foreground = TooltipForeground;
            }
        }

        public static readonly DirectProperty<StyledTextBox, IBrush> ContourBrushProperty =
            AvaloniaProperty.RegisterDirect<StyledTextBox, IBrush>(nameof(ContourBrush),
                o => o.ContourBrush);

        private IBrush _contourBrush;
        public IBrush ContourBrush
        {
            get => _contourBrush;
            set
            {
                if (_contourBrush == value)
                    return;
                _contourBrush = value;
                if (Valid)
                    setCurrentContourBrush(_contourBrush);
            }
        }

        public static readonly StyledProperty<IBrush> ContentForegroundProperty =
            AvaloniaProperty.Register<StyledTextBox, IBrush>(nameof(ContentForeground), inherits: true);

        public IBrush ContentForeground
        {
            get
            {
                return GetValue(ContentForegroundProperty);
            }
            set
            {
                SetValue(ContentForegroundProperty, value);
                _contentBox.Foreground = ContentForeground;
                _contentBox.CaretBrush = ContentForeground;
            }
        }

        public static readonly StyledProperty<double> ContentLeftPaddingProperty =
            AvaloniaProperty.Register<StyledTextBox, double>(nameof(ContentLeftPadding), inherits: true);

        public double ContentLeftPadding
        {
            get
            {
                return GetValue(ContentLeftPaddingProperty);
            }
            set
            {
                SetValue(ContentLeftPaddingProperty, value);
                _grid.Margin = new Thickness(ContentLeftPadding, 0, 0, 0);
            }
        }

        public static readonly StyledProperty<double> ImageWidthProperty =
            AvaloniaProperty.Register<StyledTextBox, double>(nameof(ImageWidth), inherits: true);

        public double ImageWidth
        {
            get
            {
                return GetValue(ImageWidthProperty);
            }
            set
            {
                SetValue(ImageWidthProperty, value);
                _imageView.Width = ImageWidth;
            }
        }

        public static readonly StyledProperty<double> ImageHeightProperty =
            AvaloniaProperty.Register<StyledTextBox, double>(nameof(ImageHeight), inherits: true);

        public double ImageHeight
        {
            get
            {
                return GetValue(ImageHeightProperty);
            }
            set
            {
                SetValue(ImageHeightProperty, value);
                _imageView.Height = ImageHeight;
            }
        }

        public static readonly StyledProperty<char> PasswordCharProperty =
            AvaloniaProperty.Register<StyledTextBox, char>(nameof(PasswordChar), inherits: true);

        public char PasswordChar
        {
            get
            {
                return GetValue(PasswordCharProperty);
            }
            set
            {
                SetValue(PasswordCharProperty, value);
                _contentBox.PasswordChar = PasswordChar;
            }
        }

        public static readonly DirectProperty<StyledTextBox, ReactiveCommand<string, bool>> ValidateCommandProperty =
            AvaloniaProperty.RegisterDirect<StyledTextBox, ReactiveCommand<string, bool>>(nameof(ValidateCommand),
                o => o.ValidateCommand,
                (o, v) => o.ValidateCommand = v);

        private ReactiveCommand<string, bool> _validateCommand;
        public ReactiveCommand<string, bool> ValidateCommand
        {
            get => _validateCommand;
            set
            {
                if (value == _validateCommand)
                    return;
                _validateCommand = value;
                _validateCommand.Execute(Text).Subscribe(b =>
                {
                    Valid = b;
                });
            }
        }

        public static readonly DirectProperty<StyledTextBox, bool> ValidProperty =
            AvaloniaProperty.RegisterDirect<StyledTextBox, bool>(nameof(Valid),
                o => o.Valid);

        private bool _valid = true;
        public bool Valid
        {
            get => _valid;
            private set
            {
                if (_valid == value)
                    return;
                bool past = _valid;
                _valid = value;
                if (_valid)
                    setCurrentContourBrush(ContourBrush);
                else if (InvalidControurBrush != null)
                    setCurrentContourBrush(InvalidControurBrush);
                RaisePropertyChanged(ValidProperty, past, _valid);
            }
        }

        public static readonly RoutedEvent<RoutedEventArgs> EnterPressedEvent =
            RoutedEvent.Register<StyledTextBox, RoutedEventArgs>(nameof(EnterPressed), RoutingStrategies.Bubble);

        public event EventHandler<RoutedEventArgs> EnterPressed
        {
            add => AddHandler(EnterPressedEvent, value);
            remove => RemoveHandler(EnterPressedEvent, value);
        }

        public static readonly DirectProperty<StyledTextBox, IBrush> InvalidBorderBrushProperty =
            AvaloniaProperty.RegisterDirect<StyledTextBox, IBrush>(nameof(InvalidControurBrush),
                o => o.InvalidControurBrush,
                (o, v) => o.InvalidControurBrush = v, Brush.Parse("#ff0000"));

        private IBrush _invalidBorderBrush;
        public IBrush InvalidControurBrush
        {
            get => _invalidBorderBrush;
            set
            {
                if (_invalidBorderBrush == value)
                    return;
                _invalidBorderBrush = value;
                if (!Valid)
                    setCurrentContourBrush(_invalidBorderBrush);
            }
        }

        protected override void OnGotFocus(GotFocusEventArgs e)
        {
            base.OnGotFocus(e);
            _carousel.SelectedIndex = 1;
            _contentBox.Focus();
        }

        private void setCurrentContourBrush(IBrush value)
        {
            _contourBorder.BorderBrush = value;
        }

        protected Carousel _carousel;
        protected Border _contourBorder;
        protected TextBox _contentBox;
        protected TextBlock _topTooltip;
        protected TextBlock _tooltip;
        protected Image _imageView;
        protected Grid _grid;

        public StyledTextBox()
        {
            InitializeComponent();

            _carousel = this.FindControl<Carousel>("Carousel");
            _contentBox = this.FindControl<TextBox>("ContentBox");
            _tooltip = this.FindControl<TextBlock>("Tooltip");
            _topTooltip = this.FindControl<TextBlock>("TopTooltip");
            _imageView = this.FindControl<Image>("Image");
            _grid = this.FindControl<Grid>("Grid");
            _contourBorder = this.FindControl<Border>("ContourBorder");

            ContourBrush = Brush.Parse(DEFAULT_CONTOUR_COLOR);
            InvalidControurBrush = Brush.Parse(DEFAULT_INVALID_CONTOUR_COLOR);

            _contentBox.GetObservable(TextBlock.TextProperty).Subscribe(
                (text) =>
                {
                    Text = text;
                });
            _contentBox.KeyDown += onKeyPress;
        }

        protected virtual void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected virtual void OnTextChange(string text)
        {
            _validateCommand?.Execute(Text).Subscribe(b => Valid = b);
            EditDone(null, null);
        }

        private void onKeyPress(object sender, KeyEventArgs args)
        {
            if (args.Key == Key.Return)
            {
                RoutedEventArgs routedEventArgs = new RoutedEventArgs()
                {
                    RoutedEvent = EnterPressedEvent,
                };
                this.RaiseEvent(routedEventArgs);
            }
        }

        public void EditDone(object sender, RoutedEventArgs args)
        {
            if (String.IsNullOrEmpty(_contentBox.Text) && !_contentBox.IsFocused)
            {
                _carousel.SelectedIndex = 0;
            }
            else
            {
                _carousel.SelectedIndex = 1;
            }
        }
    }
}
