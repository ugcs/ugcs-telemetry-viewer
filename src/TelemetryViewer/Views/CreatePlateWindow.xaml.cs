using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using UGCS.TelemetryViewer.ViewModels.Intrerfaces;

namespace UGCS.TelemetryViewer.Views
{
    public class CreatePlateWindow : Window
    {
        private bool _mouseDown;
        private Point _mouseDownPosition;
        private PointerPressedEventArgs _mouseDownArgs;
        public CreatePlateWindow()
        {
            this.initializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void initializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void CloseWithResult(object sender, RoutedEventArgs args)
        {
            Close(((ICreatePlateWindowViewModel)DataContext).TelemetryPlate);
        }

        public void CloseWithoutResult(object sender, RoutedEventArgs args)
        {
            Close(null);
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (this.IsPointerOver)
            {
                _mouseDown = true;
                _mouseDownPosition = e.GetPosition(this);
                _mouseDownArgs = e;
            }
            else
            {
                _mouseDown = false;
            }

            base.OnPointerPressed(e);
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            _mouseDown = false;
            base.OnPointerReleased(e);
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            if ((this.IsPointerOver) && _mouseDown)
            {
                var pos = e.GetPosition(this);
                if (System.Math.Abs(_mouseDownPosition.X - pos.X) > 2 
                    || System.Math.Abs(_mouseDownPosition.Y - pos.Y) > 2)
                {
                    WindowState = WindowState.Normal;
                    this.BeginMoveDrag(_mouseDownArgs);
                    _mouseDown = false;
                }
            }

            base.OnPointerMoved(e);
        }
    }
}
