using System;
using System.Diagnostics;

namespace FEM_NET.Examples
{
    internal class Timer
    {
        private readonly Stopwatch stopwatch = new Stopwatch();

        public string Name { get; }
        public TimeSpan Elapsed => stopwatch.Elapsed;

        public Timer(string name)
        {
            Name = name;
        }

        public void Start()
            => stopwatch.Start();

        public void Stop()
            => stopwatch.Stop();
    }
}
