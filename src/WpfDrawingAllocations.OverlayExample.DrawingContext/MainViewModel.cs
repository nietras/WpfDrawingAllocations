using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WpfDrawingAllocations.Common;

namespace WpfDrawingAllocations.OverlayExample.DrawingContext
{
    public class MainViewModel
        : NotifyPropertyChangedBase
    {
        const int Width = 1280;
        const int Height = 1024;
        private ICommand m_startCommand;
        WriteableBitmap m_writeableBitmap = new WriteableBitmap(Width, Height, 96, 96, PixelFormats.Gray8, BitmapPalettes.Gray256);

        readonly DrawingVisual m_drawingVisual = new DrawingVisual();

        readonly DispatcherTimer m_redrawTimer = new DispatcherTimer();
        readonly DispatcherTimer m_shutdownTimer = new DispatcherTimer();
        readonly Random random = new Random(42);

        public MainViewModel()
        {
            m_startCommand = new ActionCommand(() =>
            {
                SetupRedrawTimer();

                SetupShutdownTimer();

                StartCommand = new ActionCommand(() => { }, false);
            }, true);

            Draw();
        }

        public ICommand StartCommand
        {
            get { return m_startCommand; }
            set { m_startCommand = value; this.NotifyOfCallerMemberChanged(); }
        }

        public ImageSource ImageSource => m_writeableBitmap;

        public DrawingVisual DrawingVisual => m_drawingVisual;

        protected virtual void SetupRedrawTimer()
        {
            m_redrawTimer.Interval = TimeSpan.FromMilliseconds(1.0 / 60.0);
            m_redrawTimer.Tick += (sender, e) => Draw();
            m_redrawTimer.Start();
        }

        protected virtual void SetupShutdownTimer()
        {
            // Running for some seconds to give a profiling interval that is somewhat consistent and not full of initialization
            m_shutdownTimer.Interval = TimeSpan.FromSeconds(15);
            m_shutdownTimer.Tick += (sender, e) => Application.Current.MainWindow.Close();
            m_shutdownTimer.Start();
        }

        public void Draw()
        {
            FillBitmap();
            DrawDrawingContext();
        }
        
        private unsafe void FillBitmap()
        {
            byte fill = (byte)random.Next(byte.MaxValue / 2, byte.MaxValue / 2 + byte.MaxValue / 4);

            var bitmap = m_writeableBitmap;
            bitmap.Lock();

            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int stride = bitmap.BackBufferStride;

            byte* imagePtr = (byte*)bitmap.BackBuffer;
            for (int row = 0; row < height; row++)
            {
                var rowPtr = imagePtr + row * stride;
                for (int col = 0; col < width; col++)
                {
                    rowPtr[col] = fill;
                }
            }
            bitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
            bitmap.Unlock();
        }

        static readonly Pen RectanglePen = CreateFreezedPen(Brushes.Blue, 1);
        static readonly Pen EllipsePen = CreateFreezedPen(Brushes.Green, 1);
        static readonly Pen LinePen = CreateFreezedPen(Brushes.Red, 1);
        private void DrawDrawingContext()
        {
            const int max = Width;

            // PERF: Allocates each time and re-allocs byte[] depending on how much is drawn
            using (var dc = m_drawingVisual.RenderOpen())
            {
                for (int i = 0; i < 100; i++)
                {
                    // Rectangle
                    Rect rect = new Rect(new Point(random.Next(max), random.Next(max)), new Size(random.Next(max), random.Next(max)));
                    dc.DrawRectangle(null, RectanglePen, rect);

                    // Ellipse
                    var center = new Point(random.Next(max), random.Next(max));
                    // PERF: RotateTransform is really expensive due to boxing and what not, 
                    //       unfortunately can't create custom Transform... so caching seems only possible solution
                    dc.PushTransform(new RotateTransform(random.Next(360), center.X, center.Y));
                    dc.DrawEllipse(null, EllipsePen, center, random.Next(max) / 2, random.Next(max) / 2);
                    dc.Pop();

                    // Line
                    var p0 = new Point(random.Next(max), random.Next(max));
                    var p1 = new Point(random.Next(max), random.Next(max));
                    dc.DrawLine(LinePen, p0, p1);
                }
                // TODO: DrawText to have this in example as well
            }
        }

        // PERF: If Pen is not freezed a lot of allocations will occur
        private static Pen CreateFreezedPen(SolidColorBrush color, int thickness)
        {
            var p = new Pen(color, thickness);
            p.Freeze();
            return p;
        }
    }
}
