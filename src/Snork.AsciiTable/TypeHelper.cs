using System;
using System.Collections.Generic;

namespace Snork.AsciiTable
{
    internal static class TypeHelper
    {
        private static readonly List<Type> NumericTypes = new List<Type>()
        {
            typeof(byte), typeof(sbyte), typeof(ushort), typeof(uint), typeof(ulong), typeof(short), typeof(int),
            typeof(long), typeof(decimal), typeof(double), typeof(float)
        };

        public static bool IsNumericType(this Type type)
        {
            return NumericTypes.Contains(type);
        }
    }
}