using UnityEngine;

namespace VRC.SDK3.ClientSim.Editor.VisualElements.Fields
{
   
    public class ClientSimUINumericFieldsUtils
    { 
        public static readonly string k_AllowedCharactersForFloat = "inftynaeINFTYNAE0123456789.,-*/+%^()cosqrludxvRL=pP#";
        public static readonly string k_AllowedCharactersForByte = "0123456789-*/+%^()cosintaqrtelfundxvRL,=pPI#";
        public static readonly string k_DoubleFieldFormatString = "R";
        public static readonly string k_FloatFieldFormatString = "g7";
        public static readonly string k_ByteFieldFormatString = "###0";
        
        public static bool TryConvertStringToLong(
            string str,
            out long value)
        {
            return ExpressionEvaluator.Evaluate<long>(str, out value);
        }
        
        public static bool TryConvertStringToULong(
            string str,
            out ulong value)
        {
            return ExpressionEvaluator.Evaluate<ulong>(str, out value);
        }
        
        public static bool TryConvertStringToLong(
            string str,
            string initialValueAsString,
            out long value)
        {
            bool flag = TryConvertStringToLong(str, out value);
            long num;
            if (!flag && !string.IsNullOrEmpty(initialValueAsString) && TryConvertStringToLong(initialValueAsString, out num))
            {
                value = num;
            }
            return flag;
        }
        
        public static bool TryConvertStringToByte(
            string str,
            string initialValueAsString,
            out byte value)
        {
            long num;
            bool flag = TryConvertStringToLong(str, initialValueAsString, out num);
            value = (byte)Mathf.Clamp((int)num, (int)byte.MinValue, (int)byte.MaxValue);
            return flag;
        }
        
        public static bool TryConvertStringToSbyte(
            string str,
            string initialValueAsString,
            out sbyte value)
        {
            long num;
            bool flag = TryConvertStringToLong(str, initialValueAsString, out num);
            value = (sbyte)Mathf.Clamp((int)num, (int)sbyte.MinValue, (int)sbyte.MaxValue);
            return flag;
        }
        
        public static bool TryConvertStringToUInt(
            string str,
            string initialValueAsString,
            out uint value)
        {
            long num;
            bool flag = TryConvertStringToLong(str, initialValueAsString, out num);
            value = num < uint.MinValue ? uint.MinValue: num > uint.MaxValue ? uint.MaxValue : (uint)num;
            return flag;
        }
        
        public static bool TryConvertStringToULong(
            string str,
            string initialValueAsString,
            out ulong value)
        {
            bool flag = TryConvertStringToULong(str, out value);
            if (!flag && !string.IsNullOrEmpty(initialValueAsString) && TryConvertStringToULong(initialValueAsString, out ulong num))
            {
                value = num;
            }
            return flag;
        }
        
        public static bool TryConvertStringToShort(
            string str,
            string initialValueAsString,
            out short value)
        {
            long num;
            bool flag = TryConvertStringToLong(str, initialValueAsString, out num);
            value = num < short.MinValue ? short.MinValue: num > short.MaxValue ? short.MaxValue : (short)num;
            return flag;
        }
        
        public static bool TryConvertStringToUShort(
            string str,
            string initialValueAsString,
            out ushort value)
        {
            long num;
            bool flag = TryConvertStringToLong(str, initialValueAsString, out num);
            value = num < ushort.MinValue ? ushort.MinValue: num > ushort.MaxValue ? ushort.MaxValue : (ushort)num;
            return flag;
        }
    }
}