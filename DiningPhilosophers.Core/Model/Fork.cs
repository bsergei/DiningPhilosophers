using System.Diagnostics;
using DiningPhilosophers.Utils;

namespace DiningPhilosophers.Core.Model
{
    /// <summary>
    /// Philosopher's fork.
    /// Forks are shared resources between Philosophers.
    /// </summary>
    [DebuggerDisplay("Id={" + nameof(Id) + "}")]
    public class Fork
    {
        private static InterlockedIndex lastInstanceId_ = new InterlockedIndex(-1);

        public Fork()
        {
            Id = lastInstanceId_.GetNextAvailableIndex();
        }

        /// <summary>
        /// Unique ID.
        /// </summary>
        public int Id { get; }
    }
}