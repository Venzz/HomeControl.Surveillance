using System;
using Venz.UI.Xaml;
using Windows.UI.Xaml;

namespace HomeControl.Surveillance.Player.UI.Converters
{
    public class BooleanToVisibility: BooleanTo<Visibility> { }

    public class BooleanTo<T>: OneWayValueConverter<Boolean, T>
    {
        public T TrueValue { get; set; }
        public T FalseValue { get; set; }
        public override T Convert(Boolean convertingValue) => convertingValue ? TrueValue : FalseValue;
    }
}