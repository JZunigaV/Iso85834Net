﻿using Xunit;
using System.IO;
using Iso85834Net;
using Iso85834Net.Util;

namespace ModIso8583.Test.Parse
{
    public class TestEmptyLvars
    {
        private static readonly MessageFactory<IsoMessage> txtfact = new MessageFactory<IsoMessage>();
        private static readonly MessageFactory<IsoMessage> binfact = new MessageFactory<IsoMessage>();

        public TestEmptyLvars()
        {
            string issue38xml = @"/Resources/issue38.xml";
            txtfact.SetConfigPath(issue38xml);
            binfact.UseBinaryMessages = true;
            binfact.SetConfigPath(issue38xml);
        }

        private void CheckString(sbyte[] txt, sbyte[] bin, int field)
        {
            IsoMessage t = txtfact.ParseMessage(txt, 0);
            IsoMessage b = binfact.ParseMessage(bin, 0);
            Assert.True(t.HasField(field));
            Assert.True(b.HasField(field));
            string value = (string) (t.GetObjectValue(field));
            string valueb = (string) (b.GetObjectValue(field));
            Assert.True(value.IsEmpty());
            Assert.True(valueb.IsEmpty());
        }

        private void CheckBin(sbyte[] txt,
            sbyte[] bin,
            int field)
        {
            IsoMessage t = txtfact.ParseMessage(txt, 0);
            IsoMessage b = binfact.ParseMessage(bin, 0);
            Assert.True(t.HasField(field));
            Assert.True(b.HasField(field));
            Assert.Equal(0, ((sbyte[]) t.GetObjectValue(field)).Length);
            Assert.Equal(0, ((sbyte[]) b.GetObjectValue(field)).Length);
        }

        [Fact]
        public void TestEmptyLlvar()
        {
            IsoMessage t = txtfact.NewMessage(0x100);
            IsoMessage b = binfact.NewMessage(0x100);
            t.SetValue(2, "", IsoType.LLVAR, 0);
            b.SetValue(2, "", IsoType.LLVAR, 0);
            CheckString(t.WriteData(), b.WriteData(), 2);
        }

        [Fact]
        public void TestEmptyLllvar()
        {
            IsoMessage t = txtfact.NewMessage(0x100);
            IsoMessage b = binfact.NewMessage(0x100);
            t.SetValue(3, "", IsoType.LLLVAR, 0);
            b.SetValue(3, "", IsoType.LLLVAR, 0);
            CheckString(t.WriteData(), b.WriteData(), 3);
        }

        [Fact]
        public void TestEmptyLlllvar()
        {
            IsoMessage t = txtfact.NewMessage(0x100);
            IsoMessage b = binfact.NewMessage(0x100);
            t.SetValue(4, "", IsoType.LLLLVAR, 0);
            b.SetValue(4, "", IsoType.LLLLVAR, 0);
            CheckString(t.WriteData(), b.WriteData(), 4);
        }

        [Fact]
        public void TestEmptyLlbin()
        {
            IsoMessage t = txtfact.NewMessage(0x100);
            IsoMessage b = binfact.NewMessage(0x100);
            t.SetValue(5, new sbyte[0], IsoType.LLBIN, 0);
            b.SetValue(5, new sbyte[0], IsoType.LLBIN, 0);
            CheckBin(t.WriteData(), b.WriteData(), 5);
        }

        [Fact]
        public void TestEmptyLllbin()
        {
            IsoMessage t = txtfact.NewMessage(0x100);
            IsoMessage b = binfact.NewMessage(0x100);
            t.SetValue(6, new sbyte[0], IsoType.LLLBIN, 0);
            b.SetValue(6, new sbyte[0], IsoType.LLLBIN, 0);
            CheckBin(t.WriteData(), b.WriteData(), 6);
        }

        [Fact]
        public void TestEmptyLlllbin()
        {
            IsoMessage t = txtfact.NewMessage(0x100);
            IsoMessage b = binfact.NewMessage(0x100);
            t.SetValue(7, new sbyte[0], IsoType.LLLLBIN, 0);
            b.SetValue(7, new sbyte[0], IsoType.LLLLBIN, 0);
            CheckBin(t.WriteData(), b.WriteData(), 7);
        }
    }
}