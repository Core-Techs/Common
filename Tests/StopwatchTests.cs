using System;
using System.Threading;
using CoreTechs.Common;
using NUnit.Framework;

namespace Tests
{
    public class StopwatchTests
    {
        [Test]
        public void InitiallyStopped()
        {
            var sw = new Stopwatch();
            Assert.False(sw.IsRunning);
        }

        [Test]
        public void StartNewIsStarted()
        {
            var sw = Stopwatch.StartNew();
            Assert.True(sw.IsRunning);
        }

        [Test]
        public void CanSetElapsedOnRunningStopwatch()
        {
            var sw = Stopwatch.StartNew();
            var time = TimeSpan.FromMinutes(1);
            sw.Elapsed = time;
            Assert.True(sw.IsRunning);
            Assert.True(sw.Elapsed >= time);
        }

        [Test]
        public void CanSetElapsedOnStoppedStopwatch()
        {
            var sw = new Stopwatch();
            var time = TimeSpan.FromMinutes(1);
            sw.Elapsed = time;
            Assert.False(sw.IsRunning);
            Assert.True(sw.Elapsed == time);
        }

        [Test]
        public void CanSetTimeStarted()
        {
            var time = 30.Days();
            var started = DateTimeOffset.Now - time;

            var sw = new Stopwatch
            {
                Started = started
            };

            Assert.True(sw.IsRunning);
            Assert.True(sw.Elapsed >= time);
        }

        [Test]
        public void CanSetTimeStartedAndThenStopTheWatch()
        {
            var time = 30.Days();
            var started = DateTimeOffset.Now - time;

            var sw = new Stopwatch
            {
                Started = started
            };

            sw.Stop();

            Assert.False(sw.IsRunning);
            Assert.True(sw.Elapsed >= time);
        }

        [Test]
        public void StopwatchElapsedCorrectlyAccumulates()
        {
            var sw = Stopwatch.StartNew();
            Thread.Sleep(50);
            sw.Stop();
            Assert.True(sw.Elapsed >= 50.Milliseconds() && sw.Elapsed < 53.Milliseconds());
            Thread.Sleep(100);
            Assert.True(sw.Elapsed >= 50.Milliseconds() && sw.Elapsed < 53.Milliseconds());
            sw.Start();
            Thread.Sleep(50);
            sw.Stop();
            Assert.True(sw.Elapsed >= 100.Milliseconds() && sw.Elapsed < 106.Milliseconds());
        }
    }
}
