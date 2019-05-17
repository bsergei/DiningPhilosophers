using System;
using System.Threading;
using DiningPhilosophers.Core.Model;

namespace DiningPhilosophers.Core.Utils
{
    /// <summary>
    /// Wrapper that runs Philosopher's main routine and
    /// notify progress in separate Thread.
    /// </summary>
    public class PhilosopherRunner
    {
        private readonly Philosopher philosopher_;

        public PhilosopherRunner(Philosopher philosopher)
        {
            philosopher_ = philosopher;
        }

        /// <summary>
        /// Creates ans starts Philosopher's thread.
        /// </summary>
        public Thread Start(CancellationToken ct, IProgress<Stats> progress, Action<Exception> errorHandler)
        {
            var thread = new Thread(() => ThreadFunc(ct, progress, errorHandler));
            thread.Name = $"Philosopher_{philosopher_.Id}";
            thread.IsBackground = true;
            thread.Priority = ThreadPriority.BelowNormal;
            thread.Start();
            return thread;
        }

        private void ThreadFunc(CancellationToken ct, IProgress<Stats> progress, Action<Exception> errorHandler)
        {
            try
            {
                philosopher_.Prepare();
                while (!ct.IsCancellationRequested)
                {
                    Stats stats = philosopher_.Act();
                    progress.Report(stats);
                }
            }
            catch (Exception e)
            {
                errorHandler(e);
            }
        }
    }
}