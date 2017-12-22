using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Venz.UI.Xaml
{
    public class BooleanToVisibility: BooleanTo<Visibility> { }

    public class BooleanTo<T>: OneWayValueConverter<Boolean>
    {
        public T TrueValue { get; set; }
        public T FalseValue { get; set; }
        protected override Object Convert(Boolean value) => value ? TrueValue : FalseValue;
    }

    public abstract class OneWayValueConverter<T>: IValueConverter
    {
        protected abstract Object Convert(T convertingValue);
        public Object Convert(Object value, Type targetType, Object parameter, String language) => Convert((T)value);
        public Object ConvertBack(Object value, Type targetType, Object parameter, String language) => throw new NotImplementedException();
    }
}
