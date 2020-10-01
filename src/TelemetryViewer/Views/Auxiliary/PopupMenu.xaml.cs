using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reactive.Linq;
using MenuItem = UGCS.TelemetryViewer.Helpers.MenuItem;

namespace UGCS.TelemetryViewer.Views
{
    public class PopupMenu : UserControl
    {

        public static readonly DirectProperty<PopupMenu, AvaloniaList<MenuItem>> MenuItemsProperty =
              AvaloniaProperty.RegisterDirect<PopupMenu, AvaloniaList<MenuItem>>(nameof(MenuItems),
              getter: (o) => o.MenuItems,
              setter: (o, v) => o.MenuItems = v);

        private AvaloniaList<MenuItem> _menuItems;
        public AvaloniaList<MenuItem> MenuItems
        {
            get => _menuItems;
            set
            {
                _menuItems = value;
                if (_menuItems != null)
                {
                    rebuildView();
                    _menuItems.WeakSubscribe(onMenuItemsChanged);
                }
            }
        }

        private readonly StackPanel _menuItemsStack;
        private readonly ToggleButton _menuButton;
        private readonly Popup _popup;

        public PopupMenu()
        {
            this.initializeComponent();
            _menuItemsStack = this.FindControl<StackPanel>("MenuItemsStack");
            _menuButton = this.FindControl<ToggleButton>("OpenCloseBtn");
            _popup = this.FindControl<Popup>("MenuPopup");
            Application.Current.InputManager.PreProcess.Subscribe(e =>
            {
                if (_popup.IsOpen &&
                    (e is RawPointerEventArgs pointerEvent) &&
                    pointerEvent.Type == RawPointerEventType.LeftButtonDown &&
                    !e.Handled &&
                    !(pointerEvent.Root is PopupRoot))
                {
                    _menuButton.IsChecked = false;
                    e.Handled = true;

                }
            });
        }

        private void subscribeWindowDeactivated(Window window)
        {
            window.Deactivated += (s, e) =>
            {
                _menuButton.IsChecked = false;
            };
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            if (e.Root is Window w)
                subscribeWindowDeactivated(w);
        }

        private void rebuildView()
        {
            _menuItemsStack.Children.Clear();
            foreach (MenuItem item in _menuItems)
            {
                Control template = createMenuItemTemplate(item);
                _menuItemsStack.Children.Add(template);
            }
        }

        private void initializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void onMenuItemsChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Add)
            {
                List<Control> newControls = new List<Control>();
                foreach (var item in args.NewItems)
                {
                    newControls.Add(createMenuItemTemplate((MenuItem)item));
                }
                _menuItemsStack.Children.InsertRange(args.NewStartingIndex, newControls);
            }
            else if (args.Action == NotifyCollectionChangedAction.Remove)
            {
                _menuItemsStack.Children.RemoveRange(args.OldStartingIndex, args.OldItems.Count);
            }
        }

        private Control createMenuItemTemplate(MenuItem item)
        {
            Button rootBtn = new Button();
            rootBtn.Bind(Button.CommandProperty, item.WhenAnyValue(o => o.OnClickCommand,
                c => ReactiveCommand.CreateFromTask(async () =>
                {
                    _menuButton.IsChecked = false;
                    await c.Execute().GetAwaiter();
                })));
            rootBtn.Background = Brush.Parse("#00000000");
            rootBtn.Padding = new Thickness(0);
            rootBtn.HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Left;
            rootBtn.Cursor = Cursor.Parse("Hand");
            rootBtn.BorderThickness = new Thickness(0);

            StackPanel stack = new StackPanel
            {
                Spacing = 9,
                Margin = new Thickness(17),
                Orientation = Avalonia.Layout.Orientation.Horizontal
            };
            rootBtn.Content = stack;

            Image iconSimple = new Image();
            iconSimple.Bind(Image.SourceProperty, item.WhenAnyValue(o => o.Icon));
            iconSimple.Bind(Image.IsVisibleProperty, rootBtn.WhenAnyValue(o => o.IsPointerOver, v => !v));
            iconSimple.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
            iconSimple.Width = 22;
            iconSimple.Height = 22;
            stack.Children.Add(iconSimple);

            Image hovered = new Image();
            hovered.Bind(Image.SourceProperty, item.WhenAnyValue(o => o.HoveredIcon));
            hovered.Bind(Image.IsVisibleProperty, rootBtn.WhenAnyValue(o => o.IsPointerOver));
            hovered.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
            hovered.Width = 22;
            hovered.Height = 22;
            stack.Children.Add(hovered);

            TextBlock text = new TextBlock();
            text.Bind(TextBlock.TextProperty, item.WhenAnyValue(o => o.Title));
            text.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
            text.FontSize = 16;
            text.Bind(TextBlock.ForegroundProperty, rootBtn.WhenAnyValue(
                o => o.IsPointerOver,
                v =>
                {
                    if (v)
                        return Brush.Parse("#83cc2f");
                    else
                        return Brush.Parse("#FFFFFF");
                }));
            stack.Children.Add(text);

            return rootBtn;
        }
    }
}
