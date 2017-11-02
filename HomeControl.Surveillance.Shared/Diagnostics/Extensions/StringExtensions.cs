using System;

namespace Venz.Telemetry
{
    internal static class StringExtensions
    {
        public static String Between(this String source, String leftValue, Boolean allowFromBeginning, String rightValue, Boolean allowToEnd)
        {
            var index1 = (leftValue == null) ? -1 : source.IndexOf(leftValue);
            if (index1 == -1)
            {
                if (allowFromBeginning)
                    index1 = 0;
                else
                    return null;
            }
            else
                index1 += leftValue.Length;

            var index2 = (rightValue == null) ? -1 : source.IndexOf(rightValue, index1);
            if (index2 == -1)
            {
                if (allowToEnd)
                    index2 = source.Length;
                else
                    return null;
            }

            return source.Substring(index1, index2 - index1);
        }

        public static String Between(this String source, String leftValue, String rightValue)
        {
            return source.Between(leftValue, false, rightValue, false);
        }
    }
}
