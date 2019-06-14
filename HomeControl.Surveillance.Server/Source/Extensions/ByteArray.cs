using System;

namespace HomeControl.Surveillance
{
    public static class ByteArrayExtensions
    {
        public static String ToHexView(this Byte[] source, Boolean writeTotalSize = true) => ToHexView(source, 0, source.Length, writeTotalSize);

        public static String ToHexView(this Byte[] source, Int32 index, Boolean writeTotalSize = true) => ToHexView(source, index, source.Length, writeTotalSize);

        public static String ToHexView(this Byte[] source, Int32 index, Int32 length, Boolean writeTotalSize = true)
        {
            var representaion = "";
            var ascii = "";
            var i = 0;
            for (i = index; i < index + length; i++)
            {
                if ((i - index) % 16 == 0)
                    representaion += $"{(i - index):X4} | ";
                representaion += $"{source[i]:X2} ";
                ascii += ((source[i] > 125) || (source[i] < 32)) ? '.' : Convert.ToChar(source[i]);
                if ((i - index) % 16 == 15)
                {
                    representaion += "  " + ascii;
                    representaion += "\r\n";
                    ascii = "";
                }
            }
            if ((i - index) % 16 != 15)
            {
                while (i++ % 16 != 15)
                    representaion += "   ";
                representaion += "   ";
                representaion += "  " + ascii;
            }
            if (writeTotalSize)
                representaion += $"\r\n\r\nTotal length: {source.Length - index} bytes";
            return representaion;
        }

        public static String ToTestView(this Byte[] source) => ToTestView(source, 0, source.Length);

        public static String ToTestView(this Byte[] source, Int32 index, Int32 length)
        {
            var representaion = "";
            var i = 0;
            for (i = index; i < length; i++)
                representaion += $"{source[i]:X2} ";
            return representaion;
        }

        public static String ToByteView(this Byte[] source)
        {
            var representaion = "";
            var i = 0;
            representaion += "{ ";
            for (i = 0; i < source.Length; i++)
                representaion += $"0x{source[i]:X2}, ";
            representaion += "}";
            return representaion;
        }
    }
}
