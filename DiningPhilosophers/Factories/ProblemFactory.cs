using System.Linq;
using DiningPhilosophers.Model;
using DiningPhilosophers.Model.Hands;

namespace DiningPhilosophers.Factories
{
    /// <summary>
    /// Factory to create a gang of Philosophers with problematic hands.
    /// </summary>
    public class ProblemFactory : IDomainFactory
    {
        public Philosopher[] CreateDiningPhilosophers(int count)
        {
            var philosophers = new Philosopher[count];
            var forks = Enumerable.Range(0, count).Select(i => new Fork()).ToArray();
            for (int i = 0; i < count; i++)
            {
                var philosopher = new Philosopher(
                    new ProblemHands(forks[i], forks[(i + 1) % count]));
                philosophers[i] = philosopher;
            }
            return philosophers;
        }
    }
}