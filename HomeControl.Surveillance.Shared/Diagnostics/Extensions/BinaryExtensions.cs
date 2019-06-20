using System;
using System.IO;
using System.Text;

namespace Venz
{
    internal static class BinaryExtensions
    {
        public static String ReadUtf8String(this BinaryReader reader)
        {
            var stringLength = reader.ReadInt32();
            return Encoding.UTF8.GetString(reader.ReadBytes(stringLength));
        }

        public static void WriteUtf8String(this BinaryWriter writer, String value)
        {
            var stringData = Encoding.UTF8.GetBytes(value);
            writer.Write(stringData.Length);
            writer.Write(stringData);
        }
    }
}
