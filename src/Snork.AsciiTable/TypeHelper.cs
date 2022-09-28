using System;

namespace Snork.AsciiTable
{
    internal static class TypeHelper
    {
        public static bool HasNumericType(this object o)
        {
            if (o == null) return false;
            if (o is byte || o is sbyte || o is ushort || o is uint || o is ulong || o is short || o is int ||
                o is long || o is decimal || o is double || o is float) return true;
            switch (Type.GetTypeCode(o.GetType()))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }
    }
}