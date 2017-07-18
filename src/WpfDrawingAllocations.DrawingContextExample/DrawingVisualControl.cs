using System.Windows;
using System.Windows.Media;

namespace WpfDrawingAllocations.DrawingContextExample
{
    public class DrawingVisualControl : FrameworkElement
    {
        public DrawingVisual Visual
        {
            get { return GetValue(DrawingVisualProperty) as DrawingVisual; }
            set { SetValue(DrawingVisualProperty, value); }
        }

        private void UpdateDrawingVisual(DrawingVisual visual)
        {
            var oldVisual = Visual;
            if (oldVisual != null)
            {
                RemoveVisualChild(oldVisual);
                RemoveLogicalChild(oldVisual);
            }

            AddVisualChild(visual);
            AddLogicalChild(visual);
        }

        public static readonly DependencyProperty DrawingVisualProperty =
              DependencyProperty.Register("Visual", 
                                          typeof(DrawingVisual),
                                          typeof(DrawingVisualControl),
                                          new FrameworkPropertyMetadata(OnDrawingVisualChanged));

        private static void OnDrawingVisualChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dcv = d as DrawingVisualControl;
            if (dcv == null) { return; }

            var visual = e.NewValue as DrawingVisual;
            if (visual == null) { return; }

            dcv.UpdateDrawingVisual(visual);
        }

        protected override int VisualChildrenCount
        {
            get { return (Visual != null) ? 1 : 0; }
        }

        protected override Visual GetVisualChild(int index)
        {
            return this.Visual;
        }
    }
}
