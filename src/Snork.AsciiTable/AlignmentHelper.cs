using System;

namespace Snork.AsciiTable
{
    internal static class AlignmentHelper
    {
        public static string AlignLeft(object value, int length, char? padChar)
        {
            return (value ?? "").ToString().PadRight(length, padChar ?? ' ');
        }

        public static string AlignCenter(object value, int length, char? padChar)
        {
            var coalescedValue = (value ?? "").ToString();
            var allSpace = length - coalescedValue.Length;
            if (allSpace > 0)
            {
                var leftSpaceLeft = Convert.ToInt32(allSpace / 2.0);
                var rightSpaceLength = allSpace - leftSpaceLeft;
                return
                    $"{new string(padChar ?? ' ', leftSpaceLeft)}{coalescedValue}{new string(padChar ?? ' ', rightSpaceLength)}";
            }

            return coalescedValue;
        }

        public static string AlignAuto(Type valueType, object value, int length, char? padChar)
        {
            if (value == null) value = string.Empty;
            var asString = value.ToString();
            if (asString.Length < length)
                return valueType.IsNumericType()
                    ? AlignRight(value, length, padChar)
                    : AlignLeft(value, length, padChar);

            return asString;
        }

        public static string AlignRight(object value, int length, char? padChar = ' ')
        {
            return (value ?? "").ToString().PadLeft(length, padChar ?? ' ');
        }
    }
}