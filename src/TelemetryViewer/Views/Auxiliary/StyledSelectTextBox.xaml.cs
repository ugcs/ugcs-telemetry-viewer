using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using System.Linq;

namespace UGCS.TelemetryViewer.Views
{
    public class StyledSelectTextBox : StyledTextBox
    {
        public static readonly DirectProperty<StyledSelectTextBox, AvaloniaList<string>> VariantsProperty =
            AvaloniaProperty.RegisterDirect<StyledSelectTextBox, AvaloniaList<string>>(nameof(Variants),
                o => o.Variants,
                (o, v) => o.Variants = v,
                defaultBindingMode: BindingMode.TwoWay);

        public AvaloniaList<string> Variants { get; set; }

        protected Popup _popup;
        protected ListBox _variantsList;

        public StyledSelectTextBox() : base()
        {
            _popup = this.FindControl<Popup>("PopupList");
            _variantsList = this.FindControl<ListBox>("VariantsList");

            _variantsList.SelectionChanged += (s, e) =>
            {
                if (e.AddedItems != null && e.AddedItems.Count > 0)
                {
                    Text = (string)e.AddedItems[0];
                    hidePopup();
                }
            };

            _contentBox.GotFocus += (s, e) => showPopup();
            _contentBox.LostFocus += (s, e) => hidePopup();
        }

        protected override void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnTextChange(string text)
        {
            base.OnTextChange(text);
            showPopup();
        }

        private void showPopup()
        {
            if (Variants != null && VisualRoot is TopLevel)
            {
                _variantsList.Items = Variants.Where(s => Text == null || s.Contains(Text));
                _popup.IsOpen = true;
                _variantsList.SelectedIndex = -1;
            }
        }

        private void hidePopup()
        {
            if (_popup.IsOpen)
                _popup.IsOpen = false;
        }
    }
}
