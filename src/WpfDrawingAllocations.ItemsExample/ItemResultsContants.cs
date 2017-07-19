using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfDrawingAllocations.ItemsExample
{
    public static class ItemResultsContants
    {
        public const int RowCount = 20;
        public const int ColumnCount = 60;
        public const double ExtraRowSpaceAboveAndBelowCount = 0.5;
        public const double DistanceBetweenRows = 100.0;
        public const double DistanceBetweenColumns = 75.0;
        public const double VelocityPerSecond = 10 * DistanceBetweenColumns;
        public const double CanvasWidth = DistanceBetweenColumns* (ColumnCount + 1);
        public const double CanvasHeight = DistanceBetweenRows* (RowCount + ExtraRowSpaceAboveAndBelowCount);
    }
}
