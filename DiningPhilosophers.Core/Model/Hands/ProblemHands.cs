using System.Threading;

namespace DiningPhilosophers.Core.Model.Hands
{
    /// <summary>
    /// Naive Philosopher's hands implementation that can lead to deadlock.
    /// </summary>
    public sealed class ProblemHands : IPhilosopherHands
    {
        private readonly Fork[] forks_;

        public ProblemHands(params Fork[] forks)
        {
            forks_ = forks;
        }

        public void PickUpForks()
        {
            for (int i = 0; i < forks_.Length; i++)
            {
                if (!Monitor.TryEnter(forks_[i], 5000))
                {
                    throw new DeadlockDetectedException();
                }
            }
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