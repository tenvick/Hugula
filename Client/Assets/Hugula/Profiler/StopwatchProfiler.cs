﻿using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using UnityEngine;

namespace Hugula.Profiler
{
    [XLua.LuaCallCSharp]
    public class StopwatchProfiler : System.IDisposable
    {
        #region Static
        public static readonly Stack<StopwatchProfiler> ProfilerStack = new Stack<StopwatchProfiler>();

        /// <summary>
        /// Initializes static profiler stack with a sentry null, allows for peeking when stack is empty.
        /// </summary>
        static StopwatchProfiler()
        {
            ProfilerStack.Push(null);
        }

        #endregion Static

        #region Fields
        public string stopName = "";
        private Stopwatch stopWatch = new Stopwatch();

        private int nestingLevel = 0;

        private double maxSingleElapsedTime = 0f;

        private double childrenElapsedMilliseconds = 0f;

        private double lastElapsedMilliseconds = 0f;

        public StopwatchProfiler()
        {
            NumberOfCalls = 0;
        }

        #endregion Fields

        #region Properties
        public double MaxSingleFrameTimeInMs
        {
            get { return maxSingleElapsedTime; }
        }
        public double ElapsedMilliseconds
        {
            get { return stopWatch.Elapsed.TotalMilliseconds; }
        }

        public double ElapsedMillisecondsSelf
        {
            get { return ElapsedMilliseconds - childrenElapsedMilliseconds; }
        }

        public int NumberOfCalls { get; private set; }

        public int NumberOfCallsGreaterThan3ms { get; private set; }

        #endregion Properties

        public void Start()
        {
            UnityEngine.Profiling.Profiler.BeginSample(stopName);
#if UWATEST
				UWAEngine.PushSample (stopName);
#endif
            StopwatchProfiler lastProfiler = ProfilerStack.Peek();
            if (lastProfiler != this) ProfilerStack.Push(this);

            nestingLevel++;
            NumberOfCalls++;

            if (nestingLevel == 1)
            {

                stopWatch.Start();
            }
        }

        public void Stop()
        {
            UnityEngine.Profiling.Profiler.EndSample();
#if UWATEST
				UWAEngine.PopSample ();
#endif
            if (nestingLevel == 1)
            {
                stopWatch.Stop();

                StopwatchProfiler previousProfiler = ProfilerStack.Peek();
                if (previousProfiler == this) { ProfilerStack.Pop(); }

                previousProfiler = ProfilerStack.Peek();
                if (previousProfiler != null) previousProfiler.childrenElapsedMilliseconds += (ElapsedMilliseconds - lastElapsedMilliseconds);

                double lastFrameTime = ElapsedMilliseconds - lastElapsedMilliseconds;
                lastElapsedMilliseconds = ElapsedMilliseconds;

                if (lastFrameTime > 3.0f)
                {
                    NumberOfCallsGreaterThan3ms++;
                }
                if (lastFrameTime > maxSingleElapsedTime)
                    maxSingleElapsedTime = lastFrameTime;
            }
            nestingLevel--;
        }

        public void ForceStop()
        {
            stopWatch.Stop();
            nestingLevel = 0;
        }

        public void Reset()
        {
            stopWatch.Reset();
            nestingLevel = 0;
            NumberOfCalls = 0;
            NumberOfCallsGreaterThan3ms = 0;
            maxSingleElapsedTime = 0f;
            childrenElapsedMilliseconds = 0f;
            lastElapsedMilliseconds = 0f;
        }

        public void Dispose()
        {
            Stop();
        }
    }
}