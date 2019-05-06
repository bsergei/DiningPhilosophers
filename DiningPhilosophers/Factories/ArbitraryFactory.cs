using System.Linq;
using DiningPhilosophers.Model;
using DiningPhilosophers.Model.Hands;

namespace DiningPhilosophers.Factories
{
    /// <summary>
    /// Factory to create a gang of Philosophers with Arbitrary hands.
    /// </summary>
    public class ArbitraryFactory : IDomainFactory
    {
        public Philosopher[] CreateDiningPhilosophers(int count)
        {
            var arbitraryWaiter = new ArbitraryWaiter();

            var philosophers = new Philosopher[count];
            var forks = Enumerable.Range(0, count).Select(i => new Fork()).ToArray();
            for (int i = 0; i < count; i++)
            {
                var philosopher = new Philosopher(
                    new ArbitraryHands(arbitraryWaiter, forks[i], forks[(i + 1) % count]));
                philosophers[i] = philosopher;
            }
            return philosophers;
        }
    }
}