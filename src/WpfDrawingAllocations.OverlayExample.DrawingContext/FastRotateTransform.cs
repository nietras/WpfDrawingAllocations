//using System;
//using System.Runtime.Remoting.Channels;
//using System.Windows;
//using System.Windows.Media;
//using System.Windows.Media.Composition;

//namespace WpfDrawingAllocations.OverlayExample.DrawingContext
//{
//    // NOTE: Unfortunately can't make custom Transform due to the internal design of Transform!
//    public class FastRotateTransform : Transform
//    {
//        Matrix m_matrix = Matrix.Identity;

//        public override Matrix Value => m_matrix;

//        internal override bool IsIdentity => m_matrix.IsIdentity;

//        protected override Freezable CreateInstanceCore()
//        {
//            return new FastRotateTransform();
//        }

//        // NOTE: All the below Channel and ResourceHandle types are inaccessible.

//        internal override ResourceHandle AddRefOnChannelCore(Channel channel)
//        {
//            throw new NotImplementedException();
//        }

//        internal override Channel GetChannelCore(int index)
//        {
//            throw new NotImplementedException();
//        }

//        internal override int GetChannelCountCore()
//        {
//            throw new NotImplementedException();
//        }

//        internal override ResourceHandle GetHandleCore(Channel channel)
//        {
//            throw new NotImplementedException();
//        }

//        internal override void ReleaseOnChannelCore(Channel channel)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}