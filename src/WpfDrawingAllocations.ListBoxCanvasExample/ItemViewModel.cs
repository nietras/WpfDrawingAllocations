using System;
using System.ComponentModel;
using System.Windows.Media;
using WpfDrawingAllocations.Common;

namespace WpfDrawingAllocations.ListBoxCanvasExample
{
    public class ItemViewModel
        : NotifyPropertyChangedBase
    {
        static readonly PropertyChangedEventArgs m_relativeHorizontalPositionChanged =
            new PropertyChangedEventArgs(nameof(RelativeHorizontalPosition_mm));
        static readonly PropertyChangedEventArgs m_resultChanged =
            new PropertyChangedEventArgs(nameof(Result));
        static readonly PropertyChangedEventArgs m_drawTextResultChanged =
            new PropertyChangedEventArgs(nameof(DrawTextResult));
        static readonly PropertyChangedEventArgs m_drawColorChanged =
            new PropertyChangedEventArgs(nameof(DrawColor));
        static readonly PropertyChangedEventArgs m_backgroundColorChanged =
            new PropertyChangedEventArgs(nameof(BackgroundColor));
        static readonly PropertyChangedEventArgs m_borderColorChanged =
            new PropertyChangedEventArgs(nameof(BorderColor));

        double m_relativeHorizontalPosition_mm;
        const string NoResult = "-";
        static readonly Brush NoResultColor = Brushes.Gray;
        ItemResult? m_result = null;
        string m_drawTextResult = NoResult;
        Brush m_drawColor = NoResultColor;
        static readonly Brush BadBackgroundColor = Brushes.Red;
        static readonly Brush BadBorderColor = CreateFreezedBrush(Color.FromRgb(128, 0, 0));

        Brush m_backgroundColor = Brushes.Transparent;
        Brush m_borderColor = Brushes.Transparent;

        public double ColumnHorizontalPosition_mm { get; set; }
        public double RelativeHorizontalPosition_mm => m_relativeHorizontalPosition_mm;
        public double RowVerticalPosition_mm { get; set; }

        public Brush DrawColor
        {
            get { return m_drawColor; }
            set { m_drawColor = value; RaisePropertyChanged(m_drawColorChanged); }
        }

        public Brush BackgroundColor
        {
            get { return m_backgroundColor; }
            set { m_backgroundColor = value; RaisePropertyChanged(m_backgroundColorChanged); }
        }

        public Brush BorderColor
        {
            get { return m_borderColor; }
            set { m_borderColor = value; RaisePropertyChanged(m_borderColorChanged); }
        }

        public ItemResult? Result
        {
            get { return m_result; }
            set
            {
                m_result = value;
                RaisePropertyChanged(m_resultChanged);

                DrawTextResult = m_result?.ToText();
                DrawColor = m_result?.ToBrush();
            }
        }

        public string DrawTextResult
        {
            get { return m_drawTextResult; }
            set { m_drawTextResult = value; RaisePropertyChanged(m_drawTextResultChanged); }
        }

        public double PocketHeight_mm { get; set; }
        public double PocketWidth_mm { get; set; }

        public double ItemHeight_mm { get; set; }
        public double ItemWidth_mm { get; set; }
        public double FontSize_mm => ItemHeight_mm * 0.9;

        public void UpdateRelativeHorizontalPosition(double newPosition)
        {
            m_relativeHorizontalPosition_mm = newPosition - ColumnHorizontalPosition_mm;
            RaisePropertyChanged(m_relativeHorizontalPositionChanged);
        }

        public void Reset(double columnPosition)
        {
            ColumnHorizontalPosition_mm = columnPosition;
            DrawTextResult = NoResult;
            DrawColor = NoResultColor;
            BackgroundColor = Brushes.Transparent;
            BorderColor = Brushes.Transparent;
        }

        public void UpdateState(bool bad)
        {
            if (bad)
            {
                BackgroundColor = BadBackgroundColor;
            }
            else if (BackgroundColor != Brushes.Transparent)
            {
                BorderColor = BadBorderColor;
                BackgroundColor = Brushes.Transparent;
            }
        }

        private static Brush CreateFreezedBrush(Color color)
        {
            var b = new SolidColorBrush(color);
            b.Freeze();
            return b;
        }
    }

    public class ItemViewModelDesign : ItemViewModel
    {
        public ItemViewModelDesign()
        {
            // Position not needed in actual view model for item
            RowVerticalPosition_mm = 20;
            ItemHeight_mm = 60;
            ItemWidth_mm = 40;
        }
    }
}