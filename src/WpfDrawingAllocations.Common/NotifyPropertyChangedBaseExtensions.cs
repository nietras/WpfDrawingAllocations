using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WpfDrawingAllocations.Common
{
    public static class NotifyPropertyChangedBaseExtensions
    {
        public static void NotifyOfCallerMemberChanged(this NotifyPropertyChangedBase vm, [CallerMemberName] string memberName = "")
        {
            vm.NotifyOfPropertyChange(memberName);
        }
        public static void NotifyOfPropertyChange(this NotifyPropertyChangedBase vm, string name)
        {
            // Note occurs overhead of allocating the eventArgs object
            vm.RaisePropertyChanged(new PropertyChangedEventArgs(name));
        }

        public static PropertyChangedNotifier NotifierFromProperty(
            this NotifyPropertyChangedBase viewModel,
            string name)
        {
            return new PropertyChangedNotifier(viewModel, name);
        }
    }

    /// <summary>
    /// Notifier for avoiding heap allocations when raising event.
    /// </summary>
    public class PropertyChangedNotifier
    {
        readonly NotifyPropertyChangedBase m_viewModel;
        readonly PropertyChangedEventArgs m_args;

        public PropertyChangedNotifier(NotifyPropertyChangedBase viewModel, string propertyName)
        {
            if (viewModel == null) { throw new ArgumentNullException(nameof(viewModel)); }
            if (propertyName == null) { throw new ArgumentNullException(nameof(propertyName)); }
            m_viewModel = viewModel;
            m_args = new PropertyChangedEventArgs(propertyName);
        }

        public void Notify()
        {
            m_viewModel.RaisePropertyChanged(m_args);
        }
    }
}