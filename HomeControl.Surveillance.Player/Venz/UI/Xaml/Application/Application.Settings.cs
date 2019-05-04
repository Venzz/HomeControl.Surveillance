using System;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace Venz.UI.Xaml
{
    public class ApplicationSettings
    {
        private IPropertySet Properties = ApplicationData.Current.LocalSettings.Values;



        protected T Get<T>(String propertyName) => Get<T>(propertyName, default(T));

        protected T Get<T>(String propertyName, T defaultValue)
        {
            if (Properties.ContainsKey(propertyName))
                return (T)Properties[propertyName];
            return defaultValue;
        }

        protected DateTime Get(String propertyName, DateTime defaultValue)
        {
            if (Properties.ContainsKey(propertyName))
                return new DateTime((Int64)Properties[propertyName]);
            return defaultValue;
        }

        protected void Set(String propertyName, Object value)
        {
            if (!Properties.ContainsKey(propertyName))
                Properties.Add(propertyName, value);
            else
                Properties[propertyName] = value;
        }

        protected void Set(String propertyName, DateTime value)
        {
            if (!Properties.ContainsKey(propertyName))
                Properties.Add(propertyName, value.Ticks);
            else
                Properties[propertyName] = value.Ticks;
        }
    }
}