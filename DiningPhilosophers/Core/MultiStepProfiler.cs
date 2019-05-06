using System.Collections.Generic;
using System.Diagnostics;

namespace DiningPhilosophers.Core
{
    /// <summary>
    /// Fast accurate profiler that allows to log multiple steps.
    /// </summary>
    public class MultiStepProfiler
    {
        private readonly Stopwatch sw_;
        private readonly List<long> intervals_;
        private int? currentStep_;
        private long total_;

        public MultiStepProfiler()
        {
            sw_ = Stopwatch.StartNew();
            intervals_ = new List<long>(10);
        }

        /// <summary>
        /// Returns total elapsed ticks for specified zero-based step index.
        /// </summary>
        public long this[int step]
        {
            get { return intervals_[step]; }
        }

        /// <summary>
        /// Returns total elapsed ticks between first StartNextStep and Stop calls.
        /// </summary>
        public long Total
        {
            get { return total_; }
        }

        /// <summary>
        /// Begins new profiler session, or continue to profile next step.
        /// </summary>
        public void StartNextStep()
        {
            var ticks = sw_.ElapsedTicks;

            if (currentStep_ == null)
            {
                // Start new profiler session.
                total_ = ticks;
                currentStep_ = 0;
            }
            else
            {
                // Put elapsed ticks for prev step.
                intervals_[currentStep_.Value] = ticks - intervals_[currentStep_.Value];

                // Start new step.
                currentStep_ = currentStep_.Value + 1;
            }

            if (intervals_.Count <= currentStep_.Value)
            {
                // Expand list if need more steps.
                intervals_.Add(ticks);
            }
            else
            {
                // Put start time for new step.
                intervals_[currentStep_.Value] = ticks;
            }
        }

        /// <summary>
        /// Stops profiler session.
        /// </summary>
        public void Stop()
        {
            var ticks = sw_.ElapsedTicks;

            if (currentStep_ != null)
            {
                intervals_[currentStep_.Value] = ticks - intervals_[currentStep_.Value];
                currentStep_ = null;
            }

            total_ = ticks - total_;
        }
    }
}