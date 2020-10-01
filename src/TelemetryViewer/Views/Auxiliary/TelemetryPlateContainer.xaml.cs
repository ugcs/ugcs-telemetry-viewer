using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reactive;
using UGCS.TelemetryViewer.ViewModels.Auxiliary;

namespace UGCS.TelemetryViewer.Views
{
    public class TelemetryPlateContainer : UserControl
    {

        private const double PLATE_SIZE = 200;

        private double CellSize => PLATE_SIZE + Spacing * 2;

        private int _columnCount;

        private AddPlateControl _addButton;

        private Grid _gridContainer;

        public static readonly DirectProperty<TelemetryPlateContainer, AvaloniaList<ITelemetryPlate>> PlatesProperty =
            AvaloniaProperty.RegisterDirect<TelemetryPlateContainer, AvaloniaList<ITelemetryPlate>>(
                nameof(Plates),
                getter: (o) => o.Plates,
                setter: (o, v) => o.Plates = v);

        private AvaloniaList<ITelemetryPlate> _plates;
        public AvaloniaList<ITelemetryPlate> Plates
        {
            get => _plates;
            set
            {
                if (_plates != null)
                {
                    _plates.CollectionChanged -= onPlatesChanged;
                }
                _plates = value;
                if (_plates != null)
                {
                    _plates.CollectionChanged += onPlatesChanged;
                }
                rebuildView();
            }
        }

        public static readonly DirectProperty<TelemetryPlateContainer, ReactiveCommand<Unit, ITelemetryPlate>> AddPlateCommandProperty =
            AvaloniaProperty.RegisterDirect<TelemetryPlateContainer, ReactiveCommand<Unit, ITelemetryPlate>>(
                nameof(Plates),
                getter: (o) => o.AddPlateCommand,
                setter: (o, v) => o.AddPlateCommand = v);

        public ReactiveCommand<Unit, ITelemetryPlate> AddPlateCommand { get; set; }

        public static readonly DirectProperty<TelemetryPlateContainer, double> SpacingProperty =
            AvaloniaProperty.RegisterDirect<TelemetryPlateContainer, double>(
                nameof(Spacing),
                getter: (o) => o.Spacing,
                setter: (o, v) => o.Spacing = v);

        public double Spacing { get; set; }

        public static readonly DirectProperty<TelemetryPlateContainer, bool> IsAddEnabledProperty =
            AvaloniaProperty.RegisterDirect<TelemetryPlateContainer, bool>(
                nameof(IsAddEnabled),
                getter: (o) => o.IsAddEnabled,
                setter: (o, v) => o.IsAddEnabled = v);

        private bool _isAddEnabled;
        public bool IsAddEnabled
        {
            get
            {
                return _isAddEnabled;
            }
            set
            {
                _isAddEnabled = value;
                if (_addButton != null)
                    _addButton.ButtonEnabled = _isAddEnabled;
            }
        }

        public static readonly DirectProperty<TelemetryPlateContainer, string> DisableAddTooltipProperty =
            AvaloniaProperty.RegisterDirect<TelemetryPlateContainer, string>(
                nameof(DisableAddTooltip),
                getter: (o) => o.DisableAddTooltip,
                setter: (o, v) => o.DisableAddTooltip = v);

        private string _disableAddTooltip;
        public string DisableAddTooltip
        {
            get
            {
                return _disableAddTooltip;
            }
            set
            {
                _disableAddTooltip = value;
                if (_addButton != null)
                    _addButton.DisableTip = _disableAddTooltip;
            }
        }

        public TelemetryPlateContainer()
        {
            this.initializeComponent();
        }

        private void initializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            _gridContainer = this.FindControl<Grid>("ContainerGrid");

            this.WhenAnyValue(o => o.Bounds).Subscribe(b =>
            {
                if (Plates != null)
                    resizeView();
            });
        }

        private void setGridSize(int columnCount, int rowCount)
        {
            setColumnCount(columnCount);
            setRowCount(rowCount);
        }

        private void setColumnCount(int columnCount)
        {
            int deltaColumn = _gridContainer.ColumnDefinitions.Count - columnCount;
            if (deltaColumn > 0) // need to remove
            {
                _gridContainer.ColumnDefinitions
                    .RemoveRange(columnCount, deltaColumn);
            }
            if (deltaColumn < 0) // need to add
            {
                _gridContainer.ColumnDefinitions.AddRange(generateColumns(CellSize, -deltaColumn));
            }
        }

        private void setRowCount(int rowCount)
        {
            int deltaRow = _gridContainer.RowDefinitions.Count - rowCount;
            if (deltaRow > 0) // need to remove
            {
                _gridContainer.RowDefinitions
                    .RemoveRange(rowCount, deltaRow);
            }
            if (deltaRow < 0) // need to add
            {
                _gridContainer.RowDefinitions.AddRange(generateRows(CellSize, -deltaRow));
            }
        }

        private static IEnumerable<ColumnDefinition> generateColumns(double cellSize, int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return new ColumnDefinition(cellSize, GridUnitType.Pixel);
            }
        }

        private static IEnumerable<RowDefinition> generateRows(double cellSize, int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return new RowDefinition(cellSize, GridUnitType.Pixel);
            }
        }

        private static void calculateRowsAndColumns(double width, double cellSize, int elementsCount,
            out int rowCount, out int columnCount)
        {
            columnCount = calculateColumnCount(width, cellSize);
            rowCount = calculateRowCount(elementsCount, columnCount);
        }

        private static int calculateColumnCount(double width, double cellSize)
        {
            int columnCount = (int)(width / cellSize);
            columnCount = Math.Max(columnCount, 1);
            return columnCount;
        }

        private static int calculateRowCount(int elementsCount, int columnCount)
        {
            return elementsCount / columnCount + 1; // add one always cause "add" button
        }

        private void rebuildView()
        {
            _gridContainer.Children.Clear();

            if (Plates != null)
            {
                // calculate columns and rows count
                calculateRowsAndColumns(Bounds.Width, CellSize, Plates.Count,
                    out int rowCount, out int columnCount);

                _columnCount = columnCount;

                // resize grid
                setGridSize(columnCount, rowCount);

                // add "add" button
                _addButton = TelemetryViewerApp.Current.ViewFactory
                    .CreateUserControl<AddPlateControl>(true);
                _addButton.ButtonEnabled = _isAddEnabled;
                _addButton.DisableTip = _disableAddTooltip;
                _addButton.Click += onAddClick;
                _gridContainer.Children.Add(_addButton);
                resetAddButton(_columnCount);

                // add new contols
                for (int i = 0; i < Plates.Count; i++)
                {
                    TelemetryPlateControl control = TelemetryViewerApp.Current.ViewFactory
                        .CreateUserControl<TelemetryPlateControl>(true);
                    int col = i % _columnCount;
                    int row = i / _columnCount;
                    Grid.SetColumn(control, col);
                    Grid.SetRow(control, row);
                    control.DataContext = Plates[i];
                    _gridContainer.Children.Add(control);
                }
            }
        }

        private void resizeView()
        {
            resizeView(Bounds.Width);
        }

        private void resizeView(double width)
        {
            // calculate columns and rows count
            calculateRowsAndColumns(width, CellSize, Plates.Count,
                out int rowCount, out int columnCount);

            // do not resize if column count is the same
            if (columnCount == _columnCount)
            {
                return;
            }
            _columnCount = columnCount;

            // resize grid
            setGridSize(columnCount, rowCount);

            // reset all plates
            for (int i = 0; i < _gridContainer.Children.Count - 1; i++)
            {
                if (_gridContainer.Children[i + 1] == _addButton)
                    continue;
                TelemetryPlateControl plate =
                    (TelemetryPlateControl)_gridContainer.Children[i + 1];
                int col = i % columnCount;
                int row = i / columnCount;
                Grid.SetColumn(plate, col);
                Grid.SetRow(plate, row);
            }

            resetAddButton(columnCount);
        }

        private void onAddClick(object sender, RoutedEventArgs args)
        {
            AddPlateCommand?.Execute().Subscribe((plate) =>
            {
                if (plate != null)
                    Plates.Add(plate);
            });
        }

        private void onPlatesChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (sender != _plates)
                return;

            // row count could be changed
            setRowCount(calculateRowCount(Plates.Count, _columnCount));

            if (args.Action == NotifyCollectionChangedAction.Add)
            {
                // add new contols
                for (int i = 0; i < args.NewItems.Count; i++)
                {
                    int index = args.NewStartingIndex + i;
                    TelemetryPlateControl control = TelemetryViewerApp.Current.ViewFactory
                        .CreateUserControl<TelemetryPlateControl>(true);
                    int col = index % _columnCount;
                    int row = index / _columnCount;
                    Grid.SetColumn(control, col);
                    Grid.SetRow(control, row);
                    control.DataContext = Plates[index];
                    _gridContainer.Children.Add(control);
                }

                // move old controls
                for (int i = args.NewStartingIndex + args.NewItems.Count; // from place where new elements was added
                    i < _gridContainer.Children.Count - 1; // to the end of list
                    i++)
                {
                    TelemetryPlateControl plate =
                        (TelemetryPlateControl)_gridContainer.Children[i + 1]; // i + 1 because first child is "add" button
                    int col = i % _columnCount;
                    int row = i / _columnCount;
                    Grid.SetColumn(plate, col);
                    Grid.SetRow(plate, row);
                }
            }
            else if (args.Action == NotifyCollectionChangedAction.Remove)
            {
                // remove deleted contols
                for (int i = 0; i < args.OldItems.Count; i++)
                {
                    int index = args.OldStartingIndex + i;
                    _gridContainer.Children.RemoveAt(index + 1);
                }

                // move old controls
                for (int i = args.OldStartingIndex; // from place where old eme
                    i < _gridContainer.Children.Count - 1; // to the end of list
                    i++)
                {
                    TelemetryPlateControl plate =
                        (TelemetryPlateControl)_gridContainer.Children[i + 1]; // i + 1 because first child is "add" button
                    int col = i % _columnCount;
                    int row = i / _columnCount;
                    Grid.SetColumn(plate, col);
                    Grid.SetRow(plate, row);
                }
            }
            else if (args.Action == NotifyCollectionChangedAction.Reset)
            {
                rebuildView();
            }
            else if (args.Action == NotifyCollectionChangedAction.Move)
            {
                int from = Math.Max(args.NewStartingIndex, args.OldStartingIndex);
                int to = Math.Min(args.NewStartingIndex, args.OldStartingIndex);
                _gridContainer.Children.MoveRange(from + 1, args.OldItems.Count, to + 1); //  + 1 because first child is "add" button
                // move all controls
                for (int i = 1; //first child is "add" button
                    i < _gridContainer.Children.Count;
                    i++)
                {
                    TelemetryPlateControl plate =
                        (TelemetryPlateControl)_gridContainer.Children[i];
                    int col = (i - 1) % _columnCount;
                    int row = (i - 1) / _columnCount;
                    Grid.SetColumn(plate, col);
                    Grid.SetRow(plate, row);
                }
            }
            resetAddButton(_columnCount);
        }

        private void resetAddButton(int columnCount)
        {
            int lastCol = Plates.Count % columnCount;
            int lastRow = Plates.Count / columnCount;
            Grid.SetColumn(_addButton, lastCol);
            Grid.SetRow(_addButton, lastRow);
        }

    }
}
