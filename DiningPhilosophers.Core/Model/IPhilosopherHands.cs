namespace DiningPhilosophers.Core.Model
{
    /// <summary>
    /// These hands abstraction used by Philosopher to pick up and put down forks.
    /// </summary>
    public interface IPhilosopherHands
    {
        /// <summary>
        /// Pick up forks.
        /// Or, in other words, obtain mutually exclusive access to use forks.
        /// </summary>
        void PickUpForks();

        /// <summary>
        /// Put down forks.
        /// Or, in other words, release forks, so other Philosophers
        /// will get a chance to eat.
        /// </summary>
        void PutDownForks();
    }
}