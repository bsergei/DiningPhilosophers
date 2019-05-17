using System.Diagnostics;
using System.Runtime.CompilerServices;
using DiningPhilosophers.Core.Utils;
using DiningPhilosophers.Utils;

namespace DiningPhilosophers.Core.Model
{
    /// <summary>
    /// Philosopher class that implements Dining Philosophers routine:
    /// PickUpForks->EatSpaghetti->PutDownForks->Think.
    /// </summary>
    [DebuggerDisplay("Philosopher {" + nameof(Id) + "}")]
    public class Philosopher
    {
        private static InterlockedIndex lastInstanceId_ = new InterlockedIndex(-1);

        private readonly IPhilosopherHands philosopherHands_;
        private MultiStepProfiler profiler_;

        public Philosopher(IPhilosopherHands philosopherHands)
        {
            philosopherHands_ = philosopherHands;
            Id = lastInstanceId_.GetNextAvailableIndex(); // Increments for each new Philosopher's instance created.
        }

        /// <summary>
        /// Unique ID of the Philosopher.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Prepare philosopher to act.
        /// </summary>
        public void Prepare()
        {
            profiler_ = new MultiStepProfiler();
        }

        /// <summary>
        /// Runs Philosopher's routine and returns statistics as an artifact.
        /// </summary>
        public Stats Act()
        {
            profiler_.StartNextStep();
            philosopherHands_.PickUpForks();

            profiler_.StartNextStep();
            EatSpaghetti();

            profiler_.StartNextStep();
            philosopherHands_.PutDownForks();

            profiler_.StartNextStep();
            Think();

            profiler_.Stop();

            var stats = new Stats(
                Id,
                profiler_[0],
                profiler_[1],
                profiler_[2],
                profiler_[3],
                profiler_.Total);

            return stats;
        }

        private void EatSpaghetti()
        {
            UnitOfWork(50);
        }

        private void Think()
        {
            UnitOfWork(50);
        }

        /// <summary>
        /// Some static unit of work that Philosopher can do.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        private void UnitOfWork(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var x = i * i;
            }
        }
    }
}