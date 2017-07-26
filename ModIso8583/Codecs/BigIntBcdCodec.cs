﻿using System;
using System.Globalization;
using System.Numerics;
using Iso85834Net.Util;

namespace Iso85834Net.Codecs
{
    /// <summary>
    ///     A custom field encoder/decoder to be used with LLBIN/LLLBIN fields
    ///     hat contain BigIntegers in BCD encoding.
    /// </summary>
    public class BigIntBcdCodec : ICustomBinaryField
    {
        public object DecodeField(string val)
        {
            return new BigInteger(Convert.ToInt32(val,
                10));
        }

        public string EncodeField(object obj)
        {
            return obj.ToString();
        }

        public object DecodeBinaryField(sbyte[] bytes,
            int offset,
            int length)
        {
            return Bcd.DecodeToBigInteger(bytes,
                offset,
                length * 2);
        }

        public sbyte[] EncodeBinaryField(object val)
        {
            var value = (BigInteger) val;
            var s = value.ToString(NumberFormatInfo.InvariantInfo);
            var buf = new sbyte[s.Length / 2 + s.Length % 2];
            Bcd.Encode(s,
                buf);
            return buf;
        }
    }
}