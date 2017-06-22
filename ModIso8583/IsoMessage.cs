﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using ModIso8583.Util;
using C5;

namespace ModIso8583
{
    public class IsoMessage
    {
        private static readonly byte[] Hex = Encoding.UTF8.GetBytes(new[]
        {
            '0',
            '1',
            '2',
            '3',
            '4',
            '5',
            '6',
            '7',
            '8',
            '9',
            'A',
            'B',
            'C',
            'D',
            'E',
            'F'
        });

        /// <summary>
        ///     This is where values are stored
        /// </summary>
        private readonly IsoValue[] _fields = new IsoValue[129];

        public bool ForceStringEncoding { get; set; } = false;

        public IsoMessage() { }

        public IsoMessage(string header) { IsoHeader = header; }

        public IsoMessage(byte[] binaryHeader) { BinIsoHeader = binaryHeader; }

        public byte[] BinIsoHeader { get; set; }

        /// <summary>
        ///     Stores the optional ISO header.
        /// </summary>
        public string IsoHeader { get; set; }

        /// <summary>
        ///     Message Type
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        ///     Indicates if the message is binary-coded.
        /// </summary>
        public bool Binary { get; set; }

        /// <summary>
        ///     Sets the ETX character, which is sent at the end of the message as a terminator.
        ///     Default is -1, which means no terminator is sent.
        /// </summary>
        public int Etx { get; set; } = -1;

        /// <summary>
        ///     Flag to enforce secondary bitmap even if empty.
        /// </summary>
        public bool Forceb2 { get; set; }

        public bool BinBitmap { get; set; }

        public Encoding Encoding { get; set; } = Encoding.UTF8;

        /// <summary>
        ///     Returns the stored object value in a specified field. Fields
        ///     are represented by IsoValues which contain objects so this
        ///     method can return the contained objects directly.
        /// </summary>
        /// <param name="field">The field number (2 to 128)</param>
        /// <returns>The stored object value in that field, or null if the message does not have the field.</returns>
        public object GetObjectValue(int field)
        {
            var v = _fields[field];
            return v.Value;
        }

        /// <summary>
        ///     Returns the IsoValue used in a field to contain an object.
        /// </summary>
        /// <param name="field">The field index (2 to 128).</param>
        /// <returns>The IsoValue for the specified field.</returns>
        public IsoValue GetField(int field)
        {
            return _fields[field];
        }

        /// <summary>
        ///     Stored the field in the specified index. The first field is the secondary bitmap and has index 1,
        ///     o the first valid value for index must be 2.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="field"></param>
        /// <returns>The receiver (useful for setting several fields in sequence).</returns>
        public IsoMessage SetField(int index,
            IsoValue field)
        {
            if (index < 2 || index > 128) throw new IndexOutOfRangeException("Field index must be between 2 and 128");
            if (field != null) field.Encoding = Encoding;
            _fields[index] = field;
            return this;
        }

        /// <summary>
        ///     Convenient method for setting several fields in one call
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public IsoMessage SetFields(HashDictionary<int, IsoValue> values)
        {
            foreach (var isoValue in values)
                SetField(isoValue.Key,
                    isoValue.Value);
            return this;
        }

        /// <summary>
        ///     Sets the specified value in the specified field, creating an IsoValue internally.
        /// </summary>
        /// <param name="index">The field number (2 to 128)</param>
        /// <param name="value">The value to be stored.</param>
        /// <param name="encoder">An optional CustomField to encode/decode the value.</param>
        /// <param name="t"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public IsoMessage SetValue(int index,
            object value,
            ICustomField encoder,
            IsoType t,
            int length)
        {
            if (index < 2 || index > 128) throw new IndexOutOfRangeException("Field index must be between 2 and 128");
            if (value == null) { _fields[index] = null; }
            else
            {
                IsoValue v;
                v = t.NeedsLength() ? new IsoValue(t,
                    value,
                    length,
                    encoder) : new IsoValue(t,
                    value,
                    encoder);
                v.Encoding = Encoding;
                _fields[index] = v;
            }
            return this;
        }

        /// <summary>
        ///     Sets the specified value in the specified field, creating an IsoValue internally.
        /// </summary>
        /// <param name="index">The field number (2 to 128)</param>
        /// <param name="value">The value to be stored.</param>
        /// <param name="t">he ISO type.</param>
        /// <param name="length">The length of the field, used for ALPHA and NUMERIC values only, ignored with any other type.</param>
        /// <returns></returns>
        public IsoMessage SetValue(int index,
            object value,
            IsoType t,
            int length)
        {
            return SetValue(index,
                value,
                null,
                t,
                length);
        }

        /// <summary>
        ///     A convenience method to set new values in fields that already contain values.
        ///     The field's type, length and custom encoder are taken from the current value.
        ///     This method can only be used with fields that have been previously set,
        ///     usually from a template in the MessageFactory.
        /// </summary>
        /// <param name="index">The field's index</param>
        /// <param name="value">The new value to be set in that field.</param>
        /// <returns>The message itself.</returns>
        public IsoMessage UpdateValue(int index,
            object value)
        {
            var current = GetField(index);
            if (current == null) throw new ArgumentException("Value-only field setter can only be used on existing fields");
            SetValue(index,
                value,
                current.Encoder,
                current.Type,
                current.Length);
            GetField(index).Encoding = current.Encoding;
            return this;
        }

        /// <summary>
        ///     Returns true is the message has a value in the specified field.
        /// </summary>
        /// <param name="idx">The field number.</param>
        /// <returns></returns>
        public bool HasField(int idx)
        {
            {
                return _fields[idx] != null;
            }
        }

        /// <summary>
        ///     Writes a message to a stream, after writing the specified number of bytes indicating
        ///     the message's length. The message will first be written to an internal memory stream
        ///     which will then be dumped into the specified stream. This method flushes the stream
        ///     after the write. There are at most three write operations to the stream: one for the
        ///     length header, one for the message, and the last one with for the ETX.
        /// </summary>
        /// <param name="outs">The stream to write the message to.</param>
        /// <param name="lengthBytes">The size of the message length header. Valid ranges are 0 to 4.</param>
        public void Write(Stream outs,
            int lengthBytes)
        {
            if (lengthBytes > 4) throw new ArgumentException("The length header can have at most 4 bytes");

            var data = WriteData();
            if (lengthBytes > 0)
            {
                var l = data.Length;
                if (Etx > -1) l++;
                var buf = new byte[lengthBytes];
                var pos = 0;
                if (lengthBytes == 4)
                {
                    buf[0] = (byte) ((l & 0xff000000) >> 24);
                    pos++;
                }
                if (lengthBytes > 2)
                {
                    buf[pos] = (byte) ((l & 0xff0000) >> 16);
                    pos++;
                }
                if (lengthBytes > 1)
                {
                    buf[pos] = (byte) ((l & 0xff00) >> 8);
                    pos++;
                }
                buf[pos] = (byte) (l & 0xff);
                outs.Write(buf,
                    0,
                    buf.Length);
            }
            outs.Write(data,
                0,
                data.Length);
            //ETX
            if (Etx > -1) outs.WriteByte((byte) Etx);
            outs.Flush();
        }

        /// <summary>
        ///     Creates a BitSet for the bitmap.
        /// </summary>
        /// <returns></returns>
        protected BitArray CreateBitmapBitSet()
        {
            var bs = new BitArray(Forceb2 ? 128 : 64);
            for (var i = 2; i < 129; i++)
                if (_fields[i] != null)
                {
                    if (i > 64 && !Forceb2)
                    {
                        //Extend to 128 if needed
                        bs.Length = 128;
                        bs.Set(0, true);

                    }
                    bs.Set(i - 1, true);
                }
                    
            if (Forceb2)
            {
                bs.Set(0, true);
            }
            else if (bs.Length > 64)
            {
                //Extend to 128 if needed
                var b2 = new BitArray(128);
                b2.Or(bs);
                bs = b2;
                bs.Set(0, true);
            }
            return bs;
        }

        /// <summary>
        ///     Writes the message to a memory stream and returns a byte array with the result.
        /// </summary>
        /// <returns></returns>
        public byte[] WriteData()
        {
            var stream = new MemoryStream();
            if (IsoHeader != null)
                try
                {
                    var bytes = Encoding.GetBytes(IsoHeader);
                    stream.Write(bytes,
                        0,
                        bytes.Length);
                }
                catch (IOException)
                {
                    //should never happen, writing to a ByteArrayOutputStream
                }
            else if (BinIsoHeader != null)
                try
                {
                    stream.Write(BinIsoHeader,
                        0,
                        BinIsoHeader.Length);
                }
                catch (IOException)
                {
                    //should never happen, writing to a ByteArrayOutputStream
                }

            //Message Type
            if (Binary)
            {
                stream.WriteByte((byte) ((Type & 0xff00) >> 8));
                stream.WriteByte((byte) (Type & 0xff));
            }
            else
            {
                try
                {
                    var x = Type.ToString("x4");
                    byte[] bytes = Encoding.GetBytes(x);
                    stream.Write(bytes,
                        0,
                        bytes.Length);
                }
                catch (IOException)
                {
                    //should never happen, writing to a ByteArrayOutputStream
                }
            }
            //Bitmap
            var bits = CreateBitmapBitSet();

            // Write bitmap to stream
            if (Binary || BinBitmap)
            {
                var pos = 128;
                var b = 0;
                for (var i = 0; i < bits.Length; i++)
                {
                    if (bits.Get(i)) b |= pos;
                    pos >>= 1;
                    if (pos != 0) continue;
                    stream.WriteByte((byte) b);
                    pos = 128;
                    b = 0;
                }
            }
            else
            {
                MemoryStream stream2 = new MemoryStream();
                if (ForceStringEncoding)
                {
                    stream2 = stream;
                    stream = new MemoryStream();
                }
                var pos = 0;
                var lim = bits.Length / 4;
                for (var i = 0; i < lim; i++)
                {
                    var nibble = 0;
                    if (bits.Get(pos++)) nibble |= 8;
                    if (bits.Get(pos++)) nibble |= 4;
                    if (bits.Get(pos++)) nibble |= 2;
                    if (bits.Get(pos++)) nibble |= 1;
                    stream.WriteByte(Hex[nibble]);
                }

                if (ForceStringEncoding)
                {
                    var hb = Encoding.ASCII.GetString(stream.ToArray());
                    stream = stream2;
                    try
                    {
                        var bytes = Encoding.GetBytes(hb);
                        stream.Write(bytes,
                            0,
                            bytes.Length);
                    }
                    catch (IOException)
                    {
                        //never happen
                    }
                }
            }

            //Fields
            for (var i = 2; i < 129; i++)
            {
                var v = _fields[i];
                if (v == null) continue;
                try
                {
                    v.Write(stream,
                        Binary,
                        ForceStringEncoding);
                }
                catch (IOException)
                {
                    //should never happen, writing to a ByteArrayOutputStream
                }
            }
            Debug.Assert(stream != null,
                "stream != null");
            return stream.ToArray();
        }

        /// <summary>
        ///     Creates and returns a ByteBuffer with the data of the message, including the length header.
        ///     The returned buffer is already flipped, so it is ready to be written to a Channel.
        /// </summary>
        /// <param name="lengthBytes"></param>
        /// <returns></returns>
        public Stream WriteToBuffer(int lengthBytes)
        {
            if (lengthBytes > 4) throw new ArgumentException("The length header can have at most 4 bytes");
            var data = WriteData();
            var stream = new MemoryStream(lengthBytes + data.Length + (Etx > -1 ? 1 : 0));
            if (lengthBytes > 0)
            {
                var l = data.Length;
                if (Etx > -1) l++;
                if (lengthBytes == 4) stream.WriteByte((byte) ((l & 0xff000000) >> 24));
                if (lengthBytes > 2) stream.WriteByte((byte) ((l & 0xff0000) >> 16));
                if (lengthBytes > 1) stream.WriteByte((byte) ((l & 0xff00) >> 8));
                stream.WriteByte((byte) (l & 0xff));
            }
            stream.Write(data,
                0,
                data.Length);
            //ETX
            if (Etx > -1) stream.WriteByte((byte) Etx);

            //todo flip the stream
            return stream;
        }

        public string DebugString()
        {
            var sb = new StringBuilder();
            if (IsoHeader != null) sb.Append(IsoHeader);
            else if (BinIsoHeader != null)
                sb.Append("[0x").Append(HexCodec.HexEncode(BinIsoHeader,
                    0,
                    BinIsoHeader.Length)).Append("]");
            sb.Append(Type.ToString("x4"));
            //Bitmap
            var bs = CreateBitmapBitSet();
            var pos = 0;
            var lim = bs.Length / 4;
            for (var i = 0; i < lim; i++)
            {
                var nibble = 0;
                if (bs.Get(pos++)) nibble |= 8;
                if (bs.Get(pos++)) nibble |= 4;
                if (bs.Get(pos++)) nibble |= 2;
                if (bs.Get(pos++)) nibble |= 1;
                var string0 = Encoding.ASCII.GetString(Hex,
                    nibble,
                    1);
                sb.Append(string0);
            }

            //Fields
            for (var i = 2; i < 129; i++)
            {
                var v = _fields[i];
                if (v == null) continue;
                var desc = v.ToString();
                if (v.Type == IsoType.LLBIN || v.Type == IsoType.LLVAR) sb.Append(desc.Length.ToString("x2"));
                else if (v.Type == IsoType.LLLBIN || v.Type == IsoType.LLLVAR) sb.Append(desc.Length.ToString("x3"));
                else if (v.Type == IsoType.LLLLBIN || v.Type == IsoType.LLLLVAR) sb.Append(desc.Length.ToString("x4"));
                sb.Append(desc);
            }
            return sb.ToString();
        }

        /// <summary>
        ///     Returns true is the message contains all the specified fields.
        ///     A convenience for m.hasField(x) && m.hasField(y) && m.hasField(z) && ...
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public bool HasEveryField(params int[] idx)
        {
            return idx.All(HasField);
        }

        /// <summary>
        ///     Returns true is the message contains at least one of the specified fields.
        ///     A convenience for m.hasField(x) || m.hasField(y) || m.hasField(z) || ...
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public bool HasAnyField(params int[] idx)
        {
            return idx.Any(HasField);
        }
    }
}