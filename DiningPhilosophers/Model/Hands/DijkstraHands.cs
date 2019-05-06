using System;
using System.Linq;
using System.Threading;

namespace DiningPhilosophers.Model.Hands
{
    /// <summary>
    /// "Dijkstra" Philosopher's hands implementation.
    /// </summary>
    public sealed class DijkstraHands : IPhilosopherHands
    {
        private readonly Fork[] forks_;

        public DijkstraHands(params Fork[] forks)
        {
            forks_ = forks;

            // Sort forks by unique ID.
            Array.Sort(forks_.Select(_ => _.Id).ToArray(), forks_);
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