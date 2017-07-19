using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static WpfDrawingAllocations.ItemsExample.ItemResultsContants;

namespace WpfDrawingAllocations.ItemsExample.CanvasDirect
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        double m_currentPosition = 0.0;
        double m_latestColumnPosition = 0.0;
        Stopwatch m_stopwatchCurrentPosition = new Stopwatch();

        private readonly List<ItemResultView> m_items = new List<ItemResultView>();
        static readonly object[] RowTopPositionsAsObjects = MakeRowTopPositionsBoxed();
        static object[] MakeRowTopPositionsBoxed()
        {
            var ps = new object[RowCount];
            for (int row = 0; row < RowCount; row++)
            {
                var top = (row + ExtraRowSpaceAboveAndBelowCount * 0.5) * DistanceBetweenRows;
                // PERF: Top is boxed here, once for all items
                ps[row] = top;
            }
            return ps;
        }

        // Members for randomly generating data
        protected readonly ItemResult[] m_results = new ItemResult[RowCount];
        protected readonly bool[] m_states = new bool[RowCount];
        protected readonly Random m_random = new Random(17);

        readonly DispatcherTimer m_newPositionTimer = new DispatcherTimer();
        readonly DispatcherTimer m_shutdownTimer = new DispatcherTimer();


        public MainWindow()
        {
            InitializeComponent();

            var canvas = Canvas;

            InitializeCanvas(canvas);
        }

        public double CurrentPosition
        {
            get { return m_currentPosition; }
            set { m_currentPosition = value; m_stopwatchCurrentPosition.Restart(); }
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            SetupNewPositionTimer();

            SetupShutdownTimer();

            Start.IsEnabled = false;
        }

        void SetupNewPositionTimer()
        {
            m_newPositionTimer.Interval = TimeSpan.FromMilliseconds(1.0 / 60.0);
            m_newPositionTimer.Tick += (sender, e) => NewPosition();
            m_newPositionTimer.Start();
        }

        void SetupShutdownTimer()
        {
            // Running for some seconds to give a profiling interval that is somewhat consistent and not full of initialization
            m_shutdownTimer.Interval = TimeSpan.FromSeconds(20);
            m_shutdownTimer.Tick += (sender, e) => Application.Current.MainWindow.Close();
            m_shutdownTimer.Start();
        }

        public void NewPosition()
        {
            var seconds = m_stopwatchCurrentPosition.Elapsed.TotalSeconds;
            var offset = VelocityPerSecond * seconds;
            var newPosition = m_currentPosition + offset;
            //Trace.WriteLine($"S:{seconds} O:{offset} C:{CurrentPosition} N:{newPosition}");
            AddNewColumns(newPosition);

            UpdatePosition(newPosition);

            Canvas.InvalidateVisual();
        }

        private void InitializeCanvas(Canvas canvas)
        {
            canvas.Width = CanvasWidth;
            canvas.Height = CanvasHeight;

            InitializeItems();

            foreach (var item in m_items)
            {
                canvas.Children.Add(item.Border);
            }

            canvas.InvalidateVisual();
        }

        private void InitializeItems()
        {
            var columnEnd = ColumnCount;
            for (int i = 0; i < columnEnd; i++)
            {
                var position = DistanceBetweenColumns * i;
                AddNewColumn(position);

                if (i >= columnEnd / 2)
                {
                    var results = m_results;
                    m_random.RandomFillResults(results);
                    UpdateResults(position, results);
                }
                if (i >= (columnEnd * 3) / 4)
                {
                    var states = m_states;
                    m_random.RandomFillStates(states);
                    UpdateStates(position, states);
                    if (i % 2 == 0)
                    {
                        ItemResultsHelper.ClearStates(states);
                        UpdateStates(position, states);
                    }
                }
            }
            UpdatePosition(columnEnd * DistanceBetweenColumns);
        }

        private void AddNewColumns(double newPosition)
        {
            var previousColumnPosition = m_latestColumnPosition;
            var distanceFromPrevious = newPosition - previousColumnPosition;
            var newColumnsCount = (int)(distanceFromPrevious / DistanceBetweenColumns);
            //Trace.WriteLine($"LCP:{previousColumnPosition} DFP:{distanceFromPrevious} NCC:{newColumnsCount}");
            for (int i = 0; i < newColumnsCount; i++)
            {
                var position = previousColumnPosition + DistanceBetweenColumns * (i + 1);
                AddNewColumn(position);

                // TODO: Move fill to a couple of columns later
                m_random.RandomFillResults(m_results);
                UpdateResults(position, m_results);

                m_random.RandomFillStates(m_states);
                UpdateStates(position, m_states);
            }
        }
        public void AddNewColumn(double position)
        {
            var relativePosition = m_currentPosition - position;
            var relativePositionBoxed = (object)relativePosition;

            var columnsCount = m_items.Count / RowCount;
            // If full we simply move
            if (columnsCount >= ColumnCount)
            {
                for (int row = 0; row < RowCount; row++)
                {
                    // Or do we need to move 1
                    var beforeIndex = 0;
                    var newIndex = m_items.Count - 1;

                    //var itemBefore = m_items[0];
                    //m_items.Move(beforeIndex, newIndex);
                    var itemBefore = m_items[beforeIndex];
                    m_items.RemoveAt(beforeIndex);
                    m_items.Insert(newIndex, itemBefore);

                    var itemAfter = m_items[m_items.Count - 1];
                    Debug.Assert(object.ReferenceEquals(itemBefore, itemAfter));

                    var item = itemAfter;
                    item.Reset(position);
                    item.UpdateLeft(relativePositionBoxed);
                    //if (object.ReferenceEquals(m_selectedItem, item))
                    //{
                    //    SelectedItem = null;
                    //}
                }
            }
            else
            {
                for (int row = 0; row < RowCount; row++)
                {
                    // TODO: Add static method for construction...
                    var item = new ItemResultView(position);

                    // PERF: Canvas.SetTop calls property set internally, so double is boxed, 
                    //       we use a cache of boxed doubles for this then
                    var top = RowTopPositionsAsObjects[row];
                    item.Border.SetValue(Canvas.TopProperty, top);
                    //Canvas.SetTop(item.Border, top); // Boxes

                    item.UpdateLeft(relativePositionBoxed);

                    //Border.HeightProperty = ,
                    //    ColumnHorizontalPosition_mm = position,
                    //    RowVerticalPosition_mm = ,
                    //    // Below item could in the future be based on estimated item size
                    //    ItemHeight_mm = 60,
                    //    ItemWidth_mm = 40,
                    //};
                    //item.UpdateRelativeHorizontalPosition(CurrentPosition);
                    m_items.Add(item);
                }
            }
            m_latestColumnPosition = position;
        }

        public void UpdatePosition(double newPosition)
        {
            if (newPosition > m_currentPosition)
            {
                var diff = newPosition - m_currentPosition;
                CurrentPosition = newPosition;
                // Update existing items...
                if (m_items.Count > 0)
                {
                    var firstItem = m_items[0];
                    var itemPosition = firstItem.ColumnHorizontalPosition_mm;
                    var relativePosition = m_currentPosition - itemPosition;
                    // TODO: Mutate boxed position, reusing object
                    //var boxedPosition = firstItem.Border.GetValue(Canvas.LeftProperty)
                    var relativePositionBoxed = (object)relativePosition;
                    for (int i = 0; i < m_items.Count; i++)
                    {
                        var item = m_items[i];
                        if (item.ColumnHorizontalPosition_mm != itemPosition)
                        {
                            itemPosition = item.ColumnHorizontalPosition_mm;
                            relativePosition = m_currentPosition - itemPosition;
                            // TODO: Mutate boxed position of item!, reusing object
                            //var boxedPosition = firstItem.Border.GetValue(Canvas.LeftProperty)
                            relativePositionBoxed = (object)relativePosition;
                        }
                        item.UpdateLeft(relativePositionBoxed);
                    }
                }
            }
        }

        public void UpdateResults(double position, ItemResult[] results)
        {
            var firstItemIndex = FindFirstItemIndex(position);
            // If negative the items are no longer in the shift register UI
            if (firstItemIndex >= 0)
            {
                for (int i = 0; i < results.Length; i++)
                {
                    var item = m_items[i + firstItemIndex];
                    var r = results[i];
                    item.UpdateResult(r);
                }
            }
        }

        public void UpdateStates(double position, bool[] states)
        {
            var firstItemIndex = FindFirstItemIndex(position);
            // If negative the items are no longer in the shift register UI
            if (firstItemIndex >= 0)
            {
                for (int i = 0; i < RowCount; i++)
                {
                    var item = m_items[i + firstItemIndex];
                    var isBad = states[i];
                    item.UpdateState(isBad);
                }
            }
        }

        private int FindFirstItemIndex(double position)
        {
            for (int i = 0; i < m_items.Count; i += RowCount)
            {
                var item = m_items[i];
                if (item.ColumnHorizontalPosition_mm == position)
                {
                    return i;
                }
            }
            return -1;
        }
    }



    //public class ItemResultsColumnView
    //{
    //    public readonly ItemResultView[] Results;

    //    public ItemResultsColumnView(int count)
    //    {
    //        Results = new ItemResultView[count];
    //        for (int i = 0; i < Results.Length; i++)
    //        {
    //            Results[i] = new ItemResultView();
    //        }
    //    }
    //}

    [DebuggerDisplay("{m_relativeHorizontalPosition_mm}")]
    public class ItemResultView
    {
        // PERF: Can't make these static since UIElement can only be child of one at a time
        private readonly TextBlock NoResultTextBlock = new TextBlock()
        {
            Text = ItemResultExtensions.NoResultText,
            Foreground = ItemResultExtensions.NoResultColor,
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        // PERF: Can't make these static since UIElement can only be child of one at a time
        private static readonly ItemResult[] ItemResults = ((ItemResult[])Enum.GetValues(typeof(ItemResult)));
        private readonly Dictionary<ItemResult, TextBlock> m_resultToTextBlock =
                ItemResults.ToDictionary(r => r,
                r =>
                {
                    var tb = new TextBlock()
                    {
                        FontSize = 60 * 0.9,
                        Text = r.ToText(),
                        Foreground = r.ToBrush(),
                        HorizontalAlignment = HorizontalAlignment.Center,
                    };
                    return tb;
                });

        public readonly Border Border = new Border();
        public double ColumnHorizontalPosition_mm = 0.0;

        public ItemResultView(double position)
        {
            Border.BorderThickness = new Thickness(3);
            Border.Width = DistanceBetweenColumns;
            Border.Height = DistanceBetweenRows;

            Reset(position);
        }

        public void Reset(double columnPosition)
        {
            ColumnHorizontalPosition_mm = columnPosition;
            Border.Child = NoResultTextBlock;
            Border.Background = Brushes.Transparent;
            Border.BorderBrush = Brushes.Transparent;
            //DrawTextResult = NoResult;
            //DrawColor = NoResultColor;
            //BackgroundColor = Brushes.Transparent;
            //BorderColor = Brushes.Transparent;
        }

        //public void UpdateRelativeHorizontalPosition(double newPosition)
        //{
        //    m_relativeHorizontalPosition_mm = newPosition - ColumnHorizontalPosition_mm;
        //    Canvas.SetLeft(Border, m_relativeHorizontalPosition_mm);
        //    //RaisePropertyChanged(m_relativeHorizontalPositionChanged);
        //}
        public void UpdateLeft(object left)
        {
            Border.SetValue(Canvas.LeftProperty, left);
            //Canvas.SetLeft(Border, m_relativeHorizontalPosition_mm);
        }

        internal void UpdateResult(ItemResult r)
        {
            Border.Child = m_resultToTextBlock[r];
        }

        internal void UpdateState(bool bad)
        {
            if (bad)
            {
                Border.Background = ItemResultExtensions.BadBackgroundColor;
            }
            else if (Border.Background != Brushes.Transparent)
            {
                Border.BorderBrush = ItemResultExtensions.BadBorderColor;
                Border.Background = Brushes.Transparent;
            }
        }
    }
}
