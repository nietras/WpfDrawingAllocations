using System;
using System.Windows.Media;

namespace WpfDrawingAllocations.ListBoxCanvasExample
{
    public static class ItemResultExtensions
    {
        public static string ToText(this ItemResult r)
        {
            // PERF: Only use literal strings/constants here (NO ToString())
            switch (r)
            {
                case ItemResult.A:
                    return "A";
                case ItemResult.B:
                    return "B";
                case ItemResult.C:
                    return "C";
                case ItemResult.D:
                    return "D";
                case ItemResult.E:
                    return "E";
                case ItemResult.F:
                    return "F";
                default:
                    throw new ArgumentException($"Unsupported {r}");
            }
        }

        static readonly Brush EmptyColor = Brushes.Gray;
        static readonly Brush GoodColor = Brushes.LightGreen;
        static readonly Brush MediumColor = Brushes.LightBlue;
        static readonly Brush BadColor = Brushes.Red;
        public static Brush ToBrush(this ItemResult r)
        {
            switch (r)
            {
                // PERF: Only use literal strings/constants here (NO ToString())
                case ItemResult.A:
                case ItemResult.B:
                    return GoodColor;
                case ItemResult.C:
                case ItemResult.D:
                case ItemResult.E:
                    return MediumColor;
                case ItemResult.F:
                    return BadColor;
                default:
                    throw new ArgumentException($"Unsupported {r}");
            }
        }
    }
}
