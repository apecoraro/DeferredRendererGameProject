using System;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeskWars.core
{
    public class Timer
    {
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(
            out long lpPerformanceCount);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(
            out long lpFrequency);

        private long _startTime, _stopTime;
        private long _freq;

        // Constructor
        public Timer()
        {
            _startTime = 0;
            _stopTime  = 0;

            if (QueryPerformanceFrequency(out _freq) == false)
            {
                // high-performance counter not supported
                throw new Win32Exception();
            }
        }

        // Start the timer
        public long Start()
        {
            // lets do the waiting threads there work
            Thread.Sleep(0);

            QueryPerformanceCounter(out _startTime);
            return _startTime;
        }

        // Stop the timer
        public long Stop()
        {
            QueryPerformanceCounter(out _stopTime);
            return _stopTime;
        }

        // Returns the duration of the timer (in seconds)
        public double Duration
        {
            get
            {
                return (double)(_stopTime - _startTime) / (double) _freq;
            }
        }

    }
}
