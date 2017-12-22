using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Venz.Data
{
    public class ObservableObject: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };



        protected async void OnPropertyChanged(params String[] propertyNames) => await OnPropertyChangedAsync(propertyNames);

        protected async void OnPropertyChanged(IEnumerable<String> propertyNames) => await OnPropertyChangedAsync(propertyNames);

        protected Task OnPropertyChangedAsync(params String[] propertyNames) => OnPropertyChangedAsync((IEnumerable<String>)propertyNames);

        protected async Task OnPropertyChangedAsync(IEnumerable<String> propertyNames)
        {
            try
            {
                var dispatcher = UI.Xaml.Application.Dispatcher;
                if (dispatcher != null)
                    await dispatcher.RunAsync(() => OnPropertyChangedInternal(propertyNames)).ConfigureAwait(false);
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
