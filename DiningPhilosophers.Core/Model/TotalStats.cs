using System;

namespace DiningPhilosophers.Core.Model
{
    /// <summary>
    /// Total statistics.
    /// </summary>
    public class TotalStats
    {
        public long PickUpForksTime;

        public long PutDownForksTime;

        public long EatSpaghettiTime;

        public long ThinkTime;

        public long TotalTime;

        public long TotalCycles;

        public override string ToString()
        {
            var totalTime = TotalTime;
            if (totalTime == 0)
            {
                totalTime = Int64.MaxValue;
            }

            return
                $"Cycles={TotalCycles:N0}, PickUpForks={((decimal) PickUpForksTime) / totalTime:p2}, PutDownForks={((decimal) PutDownForksTime) / totalTime:p2}, EatingSpaghetti={((decimal) EatSpaghettiTime) / totalTime:p2}, Thinking={((decimal) ThinkTime) / totalTime:p2}";
        }
    }
}