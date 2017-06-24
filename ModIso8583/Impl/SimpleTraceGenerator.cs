﻿using System;

namespace ModIso8583.Impl
{
    public class SimpleTraceGenerator : ITraceNumberGenerator
    {
        private readonly object synclock = new object();
        private volatile int value;

        public SimpleTraceGenerator(int initialValue)
        {
            if (initialValue < 1 || initialValue > 999999) throw new ArgumentException("Initial value must be between 1 and 999999");
            value = initialValue - 1;
        }

        public int LastTrace => value;

        public int NextTrace()
        {
            lock (synclock)
            {
                value++;
                if (value > 999999) value = 1;
            }
            return value;
        }
    }
}