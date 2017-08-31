using System;
using System.Threading;

namespace WpfLiteCoinTester
{
    public static class TimingTasks
    {
        public static void TimedCall(this Action action, int minutes)
        {
            var timer = new Timer((e) =>
            {
                action();
            }, null, TimeSpan.Zero, TimeSpan.FromMinutes(minutes));
        }
    }
}