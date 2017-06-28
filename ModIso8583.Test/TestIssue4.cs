﻿using System;
using System.IO;
using Iso85834Net;
using Iso85834Net.Parse;
using Iso85834Net.Util;
using Xunit;

namespace ModIso8583.Test
{
    public class TestIssue4
    {
        [Fact]
        public void TestTextBitmap()
        {
            var tmf = new MessageFactory<IsoMessage>();
            ConfigParser.ConfigureFromClasspathConfig(tmf,
                @"/Resources/issue4.xml");
            var tm = tmf.NewMessage(0x800);
            var bb = tm.WriteToBuffer(2);
            Assert.Equal(70,
                bb.Length); //"Wrong message length for new TXT"

            var memStream = new MemoryStream();
            var binWriter = new BinaryWriter(memStream);
            foreach (var @sbyte in bb) binWriter.Write(@sbyte);

            var binReader = new BinaryReader(memStream);
            memStream.Position = 0;
            var buf = binReader.ReadBytes(2);
            Array.Reverse(buf); // due to the Big Endianness of Java

            Assert.Equal(68,
                BitConverter.ToInt16(buf,
                    0));

            var tmfp = new MessageFactory<IsoMessage>();
            ConfigParser.ConfigureFromClasspathConfig(tmfp,
                @"/Resources/issue4.xml");

            var buf2 = binReader.ReadBytes((int) (memStream.Length - memStream.Position));
            tm = tmfp.ParseMessage(buf2.ToSbytes(),
                0);

            var bbp = tm.WriteToBuffer(2);
            Assert.Equal(bb,
                bbp); // "Parsed-reencoded TXT differs from original"
        }

        [Fact]
        public void TestBinaryBitmap()
        {
            MessageFactory<IsoMessage> mf = new MessageFactory<IsoMessage>();
            ConfigParser.ConfigureFromClasspathConfig(mf, @"/Resources/issue4.xml");
            IsoMessage bm = mf.GetMessageTemplate(0x800);
            bm.BinBitmap = true;
            var bb = bm.WriteToBuffer(2);
            Assert.Equal(62, bb.Length); //"Wrong message length for new BIN"

            var memStream = new MemoryStream();
            var binWriter = new BinaryWriter(memStream);
            foreach (var @sbyte in bb) binWriter.Write(@sbyte);
            var binReader = new BinaryReader(memStream);
            memStream.Position = 0;
            var buf = binReader.ReadBytes(2);
            Array.Reverse(buf); // due to the Big Endianness of Java
            Assert.Equal(60,
                BitConverter.ToInt16(buf,
                    0));

            MessageFactory<IsoMessage> mfp = new MessageFactory<IsoMessage>();
            mfp.UseBinaryBitmap = true;
            ConfigParser.ConfigureFromClasspathConfig(mfp, @"/Resources/issue4.xml");

            var buf2 = binReader.ReadBytes((int)(memStream.Length - memStream.Position));
            bm = mfp.ParseMessage(buf2.ToSbytes(), 0);
            Assert.True(bm.BinBitmap, "Parsed message should have binary bitmap flag set");
            Assert.False(bm.Binary);
            var bbp = bm.WriteToBuffer(2);
            Assert.Equal(bb,
                bbp); // "Parsed-reencoded BIN differs from original"
        }
    }
}