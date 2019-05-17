using DiningPhilosophers.Core.Model;
using DiningPhilosophers.Core.Model.Hands;

namespace DiningPhilosophers.Core.Factories
{
    /// <summary>
    /// Factory to create a gang of Philosophers with ChandyMisra hands.
    /// </summary>
    public class ChandyMisraFactory : IDomainFactory
    {
        public Philosopher[] CreateDiningPhilosophers(int count)
        {
            Philosopher[] philosophers = new Philosopher[count];
            ChandyMisraHands[] hands = new ChandyMisraHands[count];
            for (int i = 0; i < count; i++)
            {
                var handsInstance = new ChandyMisraHands();
                hands[i] = handsInstance;
                philosophers[i] = new Philosopher(handsInstance);
            }

            AssignForks(philosophers, hands);
            return philosophers;
        }

        private static void AssignForks(Philosopher[] philosophers, ChandyMisraHands[] hands)
        {
            var count = philosophers.Length;
            for (int i = 0; i < count; i++)
            {
                var fork = new ChandyMisraFork();

                var p1 = philosophers[i];
                var h1 = hands[i];
                h1.AssignFork(fork);

                var p2 = philosophers[(i + 1) % count];
                var h2 = hands[(i + 1) % count];
                h2.AssignFork(fork);

                if (p1.Id < p2.Id)
                {
                    fork.Owner = h1;
                }
                else
                {
                    fork.Owner = h2;
                }
            }
        }
    }
}