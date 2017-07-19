using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfDrawingAllocations.DrawingContextExample
{
    public class MainViewModelDesign : MainViewModel
    {
        // Comment out to get designer to be redrawing
        protected override void SetupRedrawTimer() { }

        protected override void SetupShutdownTimer() { }
    }
}
