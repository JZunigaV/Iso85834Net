﻿using System.Text;
using Iso85834Net;
using Xunit;

namespace ModIso8583.Test
{
    public class TestHeaders
    {
        private MessageFactory<IsoMessage> mf;

        public TestHeaders()
        {
            mf = new MessageFactory<IsoMessage>
            {
                Encoding = Encoding.UTF8
            };
            mf.SetConfigPath(@"/Resources/config.xml");
        }

        [Fact]
        public void TestBinaryHeader()
        {
            IsoMessage m = mf.NewMessage(0x280);
            Assert.NotNull(m.BinIsoHeader);
            sbyte[] buf = m.WriteData();
            Assert.Equal(4 + 4 + 16 + 2, buf.Length);
            for (int i = 0; i < 4; i++)
            {
                Assert.Equal(buf[i], unchecked((sbyte)0xff));
            }
            Assert.Equal(buf[4], 0x30);
            Assert.Equal(buf[5], 0x32);
            Assert.Equal(buf[6], 0x38);
            Assert.Equal(buf[7], 0x30);
            //Then parse and check the header is binary 0xffffffff
            m = mf.ParseMessage(buf, 4, true);
            Assert.Null(m.IsoHeader);
            buf = m.BinIsoHeader;
            Assert.NotNull(buf);
            for (int i = 0; i < 4; i++)
            {
                Assert.Equal(buf[i], unchecked((sbyte)0xff));
            }
            Assert.Equal(0x280, m.Type);
            Assert.True(m.HasField(3));
        }
    }
}