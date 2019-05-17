using DiningPhilosophers.Core.Model;

namespace DiningPhilosophers.Core.Factories
{
    /// <summary>
    /// Factory to create a gang of Philosophers, their hands and forks.
    /// </summary>
    public interface IDomainFactory
    {
        Philosopher[] CreateDiningPhilosophers(int count);
    }
}