using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using WpfDrawingAllocations.Common;

namespace WpfDrawingAllocations.ListBoxCanvasExample
{
    public class MainViewModel
        : NotifyPropertyChangedBase
    {
        protected const int RowCount = 20;
        protected const int ColumnCount = 60;
        protected const double ExtraRowSpaceAboveAndBelowCount = 0.5;
        protected const double DistanceBetweenRows = 100.0;
        protected const double DistanceBetweenColumns = 75.0;
        protected const double VelocityPerSecond = 10 * DistanceBetweenColumns;

        double m_currentPosition = 0.0;
        double m_latestColumnPosition = 0.0;
        Stopwatch m_stopwatchCurrentPosition = new Stopwatch();

        private ICommand m_startCommand;
        private readonly ObservableCollection<ItemViewModel> m_items = new ObservableCollection<ItemViewModel>();

        // Members for randomly generating data
        protected readonly ItemResult[] m_results = new ItemResult[RowCount];
        protected readonly bool[] m_states = new bool[RowCount];
        protected readonly Random m_random = new Random(17);

        readonly DispatcherTimer m_newPositionTimer = new DispatcherTimer();
        readonly DispatcherTimer m_shutdownTimer = new DispatcherTimer();

        public MainViewModel()
        {
            CanvasWidth_mm = DistanceBetweenColumns * (ColumnCount + 1);
            CanvasHeight_mm = DistanceBetweenRows * (RowCount + ExtraRowSpaceAboveAndBelowCount);

            InitialFill();

            m_startCommand = new ActionCommand(() =>
            {
                SetupNewPositionTimer();

                SetupShutdownTimer();

                StartCommand = new ActionCommand(() => { }, false);
            }, true);

            m_stopwatchCurrentPosition.Start();
        }

        public ICommand StartCommand
        {
            get { return m_startCommand; }
            set { m_startCommand = value; this.NotifyOfCallerMemberChanged(); }
        }

        protected virtual void SetupNewPositionTimer()
        {
            m_newPositionTimer.Interval = TimeSpan.FromMilliseconds(1.0 / 60.0);
            m_newPositionTimer.Tick += (sender, e) => NewPosition();
            m_newPositionTimer.Start();
        }

        protected virtual void SetupShutdownTimer()
        {
            // Running for some seconds to give a profiling interval that is somewhat consistent and not full of initialization
            m_shutdownTimer.Interval = TimeSpan.FromSeconds(20);
            m_shutdownTimer.Tick += (sender, e) => Application.Current.MainWindow.Close();
            m_shutdownTimer.Start();
        }


        public double CanvasWidth_mm { get; }
        public double CanvasHeight_mm { get; }

        public double DistanceBetweenRows_mm => DistanceBetweenRows;
        public double DistanceBetweenColumns_mm => DistanceBetweenColumns;

        public double CurrentPosition
        {
            get { return m_currentPosition; }
            set { m_currentPosition = value; m_stopwatchCurrentPosition.Restart(); }
        }
        
        public ObservableCollection<ItemViewModel> Items => m_items;

        ItemViewModel m_selectedItem = null;
        public ItemViewModel SelectedItem
        {
            get { return m_selectedItem; }
            set { m_selectedItem = value; this.NotifyOfPropertyChange(nameof(SelectedItem)); }
        }

        public void InitialFill()
        {
            var columnEnd = ColumnCount;
            for (int i = 0; i < columnEnd; i++)
            {
                var position = DistanceBetweenColumns_mm * i;
                AddNewColumn(position);

                if (i >= columnEnd / 2)
                {
                    var results = m_results;
                    RandomFillResults(results);
                    UpdateResults(position, results);
                }
                if (i >= (columnEnd * 3) / 4)
                {
                    var states = m_states;
                    RandomFillStates(states);
                    UpdateStates(position, states);
                    if (i % 2 == 0)
                    {
                        ClearStates(states);
                        UpdateStates(position, states);
                    }
                }
            }
            UpdatePosition(columnEnd * DistanceBetweenColumns_mm);
        }

        public void NewPosition()
        {
            var seconds = m_stopwatchCurrentPosition.Elapsed.TotalSeconds;
            var offset = VelocityPerSecond * seconds;
            var newPosition = CurrentPosition + offset;
            //Trace.WriteLine($"S:{seconds} O:{offset} C:{CurrentPosition} N:{newPosition}");
            AddNewColumns(newPosition);

            UpdatePosition(newPosition);
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
                RandomFillResults(m_results);
                UpdateResults(position, m_results);

                RandomFillStates(m_states);
                UpdateStates(position, m_states);
            }
        }

        public void UpdatePosition(double newPosition)
        {
            if (newPosition > CurrentPosition)
            {
                var diff = newPosition - CurrentPosition;
                CurrentPosition = newPosition;
                // To throttle updating...
                //if (diff > 400)
                {
                    // Update existing items...
                    for (int i = 0; i < m_items.Count; i++)
                    {
                        var item = m_items[i];
                        item.UpdateRelativeHorizontalPosition(CurrentPosition);
                    }
                }
            }
        }

        public void AddNewColumn(double position)
        {
            var columnsCount = m_items.Count / RowCount;
            // If full we simply move
            if (columnsCount >= ColumnCount)
            {
                for (int row = 0; row < RowCount; row++)
                {
                    // Or do we need to move 1
                    var beforeIndex = 0;
                    var newIndex = m_items.Count - 1;

                    var itemBefore = m_items[0];
                    m_items.Move(beforeIndex, newIndex);
                    var itemAfter = m_items[m_items.Count - 1];
                    Debug.Assert(object.ReferenceEquals(itemBefore, itemAfter));

                    var item = itemAfter;
                    item.Reset(position);
                    item.UpdateRelativeHorizontalPosition(CurrentPosition);
                    if (object.ReferenceEquals(m_selectedItem, item))
                    {
                        SelectedItem = null;
                    }
                }
            }
            else
            {
                for (int row = 0; row < RowCount; row++)
                {
                    // TODO: Add static method for construction...
                    var item = new ItemViewModel()
                    {
                        ColumnHorizontalPosition_mm = position,
                        RowVerticalPosition_mm = (row + ExtraRowSpaceAboveAndBelowCount * 0.5) * DistanceBetweenRows,
                        // Below item could in the future be based on estimated item size
                        ItemHeight_mm = 60,
                        ItemWidth_mm = 40,
                        PocketWidth_mm = DistanceBetweenColumns_mm,
                        PocketHeight_mm = DistanceBetweenRows_mm,
                    };
                    item.UpdateRelativeHorizontalPosition(CurrentPosition);
                    m_items.Add(item);
                }
            }
            m_latestColumnPosition = position;
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
                    item.Result = r;
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

        protected void RandomFillResults(ItemResult[] results)
        {
            for (int r = 0; r < results.Length; r++)
            {
                results[r] = (ItemResult)m_random.Next((int)ItemResult.F);
            }
        }

        protected void RandomFillStates(bool[] states)
        {
            for (int r = 0; r < states.Length; r++)
            {
                states[r] = m_random.Next(4) == 0;
            }
        }

        protected static void ClearStates(bool[] states)
        {
            Array.Clear(states, 0, states.Length);
        }
    }
}
