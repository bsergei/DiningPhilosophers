using DiningPhilosophers.Model;

namespace DiningPhilosophers.Factories
{
    /// <summary>
    /// Factory to create a gang of Philosophers.
    /// </summary>
    public interface IDomainFactory
    {
        Philosopher[] CreateDiningPhilosophers(int count);
    }
}