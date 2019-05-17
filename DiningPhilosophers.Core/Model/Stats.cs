namespace DiningPhilosophers.Core.Model
{
    /// <summary>
    /// Philosopher's routine statistics data class.
    /// </summary>
    public struct Stats
    {
        /// <summary>
        /// Philosopher's ID.
        /// </summary>
        public readonly int PhilosopherId;

        /// <summary>
        /// Time in ticks elapsed while doing PickUpForks.
        /// </summary>
        public readonly long PickUpForksTime;

        /// <summary>
        /// Time in ticks elapsed while doing EatSpaghetti.
        /// </summary>
        public readonly long EatSpaghettiTime;

        /// <summary>
        /// Time in ticks elapsed while doing PutDownForks.
        /// </summary>
        public readonly long PutDownForksTime;

        /// <summary>
        /// Time in ticks elapsed while doing Think.
        /// </summary>
        public readonly long ThinkTime;

        /// <summary>
        /// Total time elapsed in ticks.
        /// </summary>
        public readonly long FullTime;

        public Stats(int philosopherId, long pickUpForksTime, long eatSpaghettiTime, long putDownForksTime,
            long thinkTime, long fullTime)
        {
            PhilosopherId = philosopherId;
            PickUpForksTime = pickUpForksTime;
            EatSpaghettiTime = eatSpaghettiTime;
            PutDownForksTime = putDownForksTime;
            ThinkTime = thinkTime;
            FullTime = fullTime;
        }
    }
}