using System.Diagnostics;

namespace DiningPhilosophers.Model
{
    /// <summary>
    /// Philosopher's fork.
    /// Forks are shared resources between Philosophers.
    /// </summary>
    [DebuggerDisplay("Id={" + nameof(Id) + "}")]
    public class Fork
    {
        private static int lastInstanceId_;

        public Fork()
        {
            Id = lastInstanceId_++;
        }

        /// <summary>
        /// Unique ID.
        /// </summary>
        public int Id { get; }
    }
}