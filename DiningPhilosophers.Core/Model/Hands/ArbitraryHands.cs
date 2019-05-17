using System.Threading;

namespace DiningPhilosophers.Core.Model.Hands
{
    /// <summary>
    /// "Arbitrary" Philosopher's hands implementation.
    /// </summary>
    public sealed class ArbitraryHands : IPhilosopherHands
    {
        private readonly Fork[] forks_;
        private readonly ArbitraryWaiter arbitraryWaiter_;

        public ArbitraryHands(ArbitraryWaiter arbitraryWaiter, params Fork[] forks)
        {
            arbitraryWaiter_ = arbitraryWaiter;
            forks_ = forks;
        }

        public void PickUpForks()
        {
            lock (arbitraryWaiter_) // Ask waiter to give me all forks.
            {
                for (int i = 0; i < forks_.Length; i++)
                {
                    Monitor.Enter(forks_[i]);
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