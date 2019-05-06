using System.Threading;

namespace DiningPhilosophers.Model.Hands
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
                Monitor.Enter(forks_[i]);
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