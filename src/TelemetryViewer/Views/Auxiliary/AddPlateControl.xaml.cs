using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;

namespace UGCS.TelemetryViewer.Views
{
    public class AddPlateControl : UserControl
    {

        private Button _addBtn;
        private TextBlock _disableTip;

        public static readonly RoutedEvent<RoutedEventArgs> ClickEvent =
            RoutedEvent.Register<StyledTextBox, RoutedEventArgs>(nameof(Click), RoutingStrategies.Bubble);

        public event EventHandler<RoutedEventArgs> Click
        {
            add => _addBtn.Click += value;
            remove => _addBtn.Click -= value;
        }

        public AddPlateControl()
        {
            this.initializeComponent();
        }

        private void initializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            _addBtn = this.FindControl<Button>("AddBtn");
            _disableTip = this.FindControl<TextBlock>("DisableTip");
        }

        public bool ButtonEnabled
        {
            get => _addBtn.IsEnabled;
            set => _addBtn.IsEnabled = value;
        }
        public string DisableTip
        {
            get => _disableTip.Text;
            set => _disableTip.Text = value;
        }
    }
}
