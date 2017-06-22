﻿using ModIso8583.Util;
using Xunit;

namespace ModIso8583.Test.Util
{
    public class TestBcd
    {
        [Fact]
        public void TestEncoding()
        {
            byte[] buf = new byte[2];
            buf[0] = 1; buf[1] = 1;
            Bcd.Encode("00", buf);
            Assert.Equal(new byte[] { 0, 1 }, buf);
            Bcd.Encode("79", buf);
            Assert.Equal(new byte[] { 0x79, 1 }, buf);
            Bcd.Encode("80", buf);
            Assert.Equal(new byte[] { (byte)0x80, 1 }, buf);
            Bcd.Encode("99", buf);
            Assert.Equal(new byte[] { (byte)0x99, 1 }, buf);
            Bcd.Encode("100", buf);
            Assert.Equal(new byte[] { 1, 0 }, buf);
            Bcd.Encode("779", buf);
            Assert.Equal(new byte[] { 7, 0x79 }, buf);
            Bcd.Encode("999", buf);
            Assert.Equal(new byte[] { 9, (byte)0x99 }, buf);
        }

        [Fact]
        public void TestDecoding()
        {
            byte[] buf = new byte[2];
            Assert.Equal(0, Bcd.DecodeToLong(buf, 0, 1));
            Assert.Equal(0, Bcd.DecodeToLong(buf, 0, 2));
            Assert.Equal(0, Bcd.DecodeToLong(buf, 0, 3));
            Assert.Equal(0, Bcd.DecodeToLong(buf, 0, 4));
            buf[0] = 0x79;
            Assert.Equal(79, Bcd.DecodeToLong(buf, 0, 2));
            buf[0] = (byte)0x80;
            Assert.Equal(80, Bcd.DecodeToLong(buf, 0, 2));
            buf[0] = (byte)0x99;
            Assert.Equal(99, Bcd.DecodeToLong(buf, 0, 2));
            buf[0] = 1;
            Assert.Equal(100, Bcd.DecodeToLong(buf, 0, 4));
            buf[1] = 0x79;
            Assert.Equal(179, Bcd.DecodeToLong(buf, 0, 4));
            buf[1] = (byte)0x99;
            Assert.Equal(199, Bcd.DecodeToLong(buf, 0, 4));
            buf[0] = 9;
            Assert.Equal(999, Bcd.DecodeToLong(buf, 0, 4));

        }


    }
}