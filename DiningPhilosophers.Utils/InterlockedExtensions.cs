using System.Threading;

namespace DiningPhilosophers.Utils
{
    /// <summary>
    /// Thread-safe getter for next available index.
    /// </summary>
    public struct InterlockedIndex
    {
        public volatile int Value;

        public InterlockedIndex(int initial) : this()
        {
            Value = initial;
        }

        public int GetNextAvailableIndex()
        {
            int idx;
            int initialIdx;
            do
            {
                initialIdx = Value;
                idx = initialIdx + 1;
            } while (Interlocked.CompareExchange(ref Value, idx, initialIdx) != initialIdx);

            return idx;
        }
    }
}