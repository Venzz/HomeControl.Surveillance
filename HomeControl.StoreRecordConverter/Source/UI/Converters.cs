using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace HomeControl.StoreRecordConverter.UI
{
    public class ProgressToCompletedIconVisibility: OneWayValueConverter<Decimal>
    {
        protected override Object Convert(Decimal value) => (value == 1) ? Visibility.Visible : Visibility.Collapsed;
    }

    public class ProgressToNotStartedIconVisibility: OneWayValueConverter<Decimal>
    {
        protected override Object Convert(Decimal value) => (value == 0) ? Visibility.Visible : Visibility.Collapsed;
    }

    public class ProgressToTextVisibility: OneWayValueConverter<Decimal>
    {
        protected override Object Convert(Decimal value) => (value == 1 || value == 0) ? Visibility.Collapsed : Visibility.Visible;
    }

    public class ProgressToText: OneWayValueConverter<Decimal>
    {
        protected override Object Convert(Decimal value) => $"{(Int32)(value * 100):D2}%";
    }



    public abstract class OneWayValueConverter<T>: IValueConverter
    {
        protected abstract Object Convert(T convertingValue);

        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            return Convert((T)value);
        }

        public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}