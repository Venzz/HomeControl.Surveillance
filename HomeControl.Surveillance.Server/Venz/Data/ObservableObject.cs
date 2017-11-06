using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

namespace Venz.Data
{
    public class ObservableObject: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };



        protected void OnPropertyChanged(params String[] propertyNames) => OnPropertyChanged((IEnumerable<String>)propertyNames);

        protected void OnPropertyChanged(IEnumerable<String> propertyNames)
        {
            try
            {
                var dispatcher = Application.Current?.Dispatcher;
                if (dispatcher != null)
                    dispatcher.Invoke(() => OnPropertyChangedInternal(propertyNames), DispatcherPriority.Normal);
                else
                    OnPropertyChangedInternal(propertyNames);
            }
            #if DEBUG
            catch (Exception exception)
            {
                throw exception;
            }
            #else
            catch (Exception)
            {
            }
            #endif
        }



        private void OnPropertyChangedInternal(params String[] propertyNames) => OnPropertyChangedInternal((IEnumerable<String>)propertyNames);

        private void OnPropertyChangedInternal(IEnumerable<String> propertyNames)
        {
            foreach (var propertyName in propertyNames)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
