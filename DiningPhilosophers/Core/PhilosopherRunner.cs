using System;
using System.Threading;
using DiningPhilosophers.Model;

namespace DiningPhilosophers.Core
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
        public void Start(CancellationToken ct, IProgress<Stats> progress)
        {
            var thread = new Thread(() => ThreadFunc(ct, progress));
            thread.Name = $"Philosopher_{philosopher_.Id}";
            thread.IsBackground = true;
            thread.Priority = ThreadPriority.BelowNormal;
            thread.Start();
        }

        private void ThreadFunc(CancellationToken ct, IProgress<Stats> progress)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    Stats stats = philosopher_.Act();
                    progress?.Report(stats);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}