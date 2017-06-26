﻿using System;
using System.Text;
using ModIso8583.Util;

namespace ModIso8583.Parse
{
    public class LllbinParseInfo : FieldParseInfo
    {
        public LllbinParseInfo() : base(IsoType.LLLBIN,
            0)
        { }

        public override IsoValue Parse(int field,
            sbyte[] buf,
            int pos,
            ICustomField custom)
        {
            if (pos < 0) throw new ParseException($"Invalid LLLBIN field {field} pos {pos}");
            if (pos + 3 > buf.Length) throw new ParseException($"Insufficient LLLBIN header field {field}");

            var l = DecodeLength(buf,
                pos,
                3);
            if (l < 0) throw new ParseException($"Invalid LLLBIN length {l} field {field} pos {pos}");
            if (l + pos + 3 > buf.Length) throw new ParseException($"Insufficient data for LLLBIN field {field}, pos {pos}");

            var binval = l == 0 ? new sbyte[0] : HexCodec.HexDecode(buf.SbyteString(pos + 3,
                l,
                Encoding.Default));
            if (custom == null)
                return new IsoValue(IsoType,
                    binval,
                    binval.Length);
            var customBinaryField = custom as ICustomBinaryField;
            if (customBinaryField != null)
                try
                {
                    var dec = customBinaryField.DecodeBinaryField(buf,
                        pos + 3,
                        l);
                    return dec == null ? new IsoValue(IsoType,
                        binval,
                        binval.Length) : new IsoValue(IsoType,
                        dec,
                        0,
                        custom);
                }
                catch (Exception) { throw new ParseException($"Insufficient data for LLLBIN field {field}, pos {pos}"); }
            try
            {
                var dec = custom.DecodeField(l == 0 ? "" : buf.SbyteString(pos + 3,
                    l,
                    Encoding.Default));
                return dec == null ? new IsoValue(IsoType,
                    binval,
                    binval.Length) : new IsoValue(IsoType,
                    dec,
                    l,
                    custom);
            }
            catch (Exception) { throw new Exception($"Insufficient data for LLLBIN field {field}, pos {pos}"); }
        }

        public override IsoValue ParseBinary(int field,
            sbyte[] buf,
            int pos,
            ICustomField custom)
        {
            var sbytes = buf;
            if (pos < 0) throw new ParseException($"Invalid bin LLLBIN field {field} pos {pos}");
            if (pos + 2 > buf.Length) throw new ParseException($"Insufficient LLLBIN header field {field}");
            var l = (sbytes[pos] & 0x0f) * 100 + ((sbytes[pos + 1] & 0xf0) >> 4) * 10 + (sbytes[pos + 1] & 0x0f);
            if (l < 0) throw new ParseException($"Invalid LLLBIN length {l} field {field} pos {pos}");
            if (l + pos + 2 > buf.Length) throw new ParseException($"Insufficient data for bin LLLBIN field {field}, pos {pos} requires {l}, only {buf.Length - pos + 1} available");

            var v = new sbyte[l];
            Array.Copy(sbytes,
                pos + 2,
                v,
                0,
                l);
            if (custom == null)
                return new IsoValue(IsoType,
                    v);
            var binaryField = custom as ICustomBinaryField;
            if (binaryField != null)
                try
                {
                    var dec = binaryField.DecodeBinaryField(sbytes,
                        pos + 2,
                        l);
                    return dec == null ? new IsoValue(IsoType,
                        v,
                        v.Length) : new IsoValue(IsoType,
                        dec,
                        l,
                        custom);
                }
                catch (Exception) { throw new ParseException($"Insufficient data for LLLBIN field {field}, pos {pos}"); }
            {
                var dec = custom.DecodeField(HexCodec.HexEncode(v,
                    0,
                    v.Length));
                return dec == null ? new IsoValue(IsoType,
                    v) : new IsoValue(IsoType,
                    dec,
                    custom);
            }
        }
    }
}