using System.Linq;
using DiningPhilosophers.Core.Model;
using DiningPhilosophers.Core.Model.Hands;

namespace DiningPhilosophers.Core.Factories
{
    /// <summary>
    /// Factory to create a gang of Philosophers with Dijkstra hands.
    /// </summary>
    public class DijkstraFactory : IDomainFactory
    {
        public Philosopher[] CreateDiningPhilosophers(int count)
        {
            var philosophers = new Philosopher[count];
            var forks = Enumerable.Range(0, count).Select(i => new Fork()).ToArray();
            for (int i = 0; i < count; i++)
            {
                var philosopher = new Philosopher(
                    new DijkstraHands(forks[i], forks[(i + 1) % count]));
                philosophers[i] = philosopher;
            }

            return philosophers;
        }
    }
}