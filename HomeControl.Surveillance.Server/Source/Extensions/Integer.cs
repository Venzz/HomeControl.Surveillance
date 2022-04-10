using System;

namespace HomeControl.Surveillance
{
    public static class Integer
    {
        public static String ToDataLength(this Byte dataLength) => ToDataLengthInternal(dataLength);

        public static String ToDataLength(this Int16 dataLength) => ToDataLengthInternal(dataLength);

        public static String ToDataLength(this UInt16 dataLength) => ToDataLengthInternal(dataLength);

        public static String ToDataLength(this Int32 dataLength) => ToDataLengthInternal(dataLength);

        public static String ToDataLength(this UInt32 dataLength) => ToDataLengthInternal(dataLength);

        public static String ToDataLength(this Int64 dataLength) => ToDataLengthInternal(dataLength);

        public static String ToDataLength(this UInt64 dataLength) => ToDataLengthInternal((Int64)dataLength);

        public static String ToDataLengthInternal(Int64 dataLength)
        {
            if (dataLength >= 1024 * 1024 * 1024)
                return $"{((Double)dataLength / 1024 / 1024 / 1024):0.00} gb";
            else if (dataLength >= 1024 * 1024)
                return $"{((Double)dataLength / 1024 / 1024):0.00} mb";
            else if (dataLength >= 1024)
                return $"{((Double)dataLength / 1024):0.00} kb";
            else
                return $"{dataLength:0.00} b";
        }
    }
}
