using System;
using System.Threading;

namespace DiningPhilosophers.Model.Hands
{
    /// <summary>
    /// "Agile" Philosopher's hands implementation.
    /// Philosopher with AgileHands can eat with any count of shared forks.
    /// </summary>
    public sealed class AgileHands : IPhilosopherHands
    {
        private readonly Fork[] forks_;

        public AgileHands(params Fork[] forks)
        {
            forks_ = forks;
        }

        public void PickUpForks()
        {
            bool got;
            do
            {
                got = true;
                int failedIdx = Int32.MaxValue;
                for (int i = 0; i < forks_.Length; i++)
                {
                    // Try get forks sequentially and break if failed to get any fork.
                    if (!Monitor.TryEnter(forks_[i]))
                    {
                        failedIdx = i;
                        break;
                    }
                }

                if (failedIdx != Int32.MaxValue)
                {
                    // Rollback all locks we get.
                    got = false;
                    for (int i = failedIdx - 1; i >= 0; i--)
                    {
                        Monitor.Exit(forks_[i]);
                    }
                }
            } while (!got);
        }

        public void PutDownForks()
        {
            for (int i = 0; i < forks_.Length; i++)
            {
                Monitor.Exit(forks_[i]);
            }
        }
    }
}