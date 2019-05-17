using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DiningPhilosophers.Core.Model;
using DiningPhilosophers.Sim.Model;
using DiningPhilosophers.Sim.Repositories;
using DiningPhilosophers.Utils;

namespace DiningPhilosophers.Sim.Services
{
    public class ReportingService : IReportingService
    {
        private const int StateRepositoryUpdateInterval = 1000;
        private const int DeadlockTimeout = 5000;

        private readonly IStateRepository stateRepository_;
        private readonly BufferedProducerConsumer<Stats> buffer_;
        
        private DateTime lastReported_;

        public ReportingService(IStateRepository stateRepository)
        {
            stateRepository_ = stateRepository;
            buffer_ = new BufferedProducerConsumer<Stats>();
        }

        /// <summary>
        /// This is called by each philosopher at the table.
        /// </summary>
        public void Report(Stats value)
        {
            buffer_.Push(value);
            lastReported_ = DateTime.UtcNow; // Used to track possible deadlocks (e.g., "Problem" table).
        }

        public async Task ReceiveStats(Guid tableId, TableType tableType, CancellationToken ct)
        {
            using (var deadlockCts = new CancellationTokenSource()) // CTS to cancel this task when deadlock is detected.
            {
                using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(deadlockCts.Token, ct))
                {
                    using (linkedCts.Token.Register(() => buffer_.Finish())) // Notify producer-consumer to finish and flush its buffers.
                    {
                        var startTime = DateTime.UtcNow;
                        DateTime? lastStateUpdated = null;

                        var statistics = new Dictionary<int, TotalStats>();

                        bool? available;
                        while ((available = await buffer_.Source.OutputAvailableAsync(1000)) ?? true)
                        {
                            if (available != null)
                            {
                                while (buffer_.Source.TryReceiveAll(out var statsArray))
                                {
                                    for (var i1 = 0; i1 < statsArray.Count; i1++)
                                    {
                                        var statsArr = statsArray[i1];
                                        for (var i2 = 0; i2 < statsArr.Length; i2++)
                                        {
                                            var stats = statsArr[i2];
                                            AggregateStats(statistics, stats);
                                        }
                                    }
                                }
                            }

                            lastStateUpdated = await SendToStateRepository(
                                tableId, tableType, startTime, null, statistics,
                                lastStateUpdated, deadlockCts);
                        }

                        await SendToStateRepository(
                            tableId, tableType, startTime, DateTime.UtcNow, statistics, 
                            null, deadlockCts);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private async Task<DateTime?> SendToStateRepository(
            Guid tableId,
            TableType tableType,
            DateTime startTime,
            DateTime? endTime,
            Dictionary<int, TotalStats> statistics,
            DateTime? lastStateUpdated, 
            CancellationTokenSource deadlockCts)
        {
            bool isUpdated = false;
            var currentTimeStamp = DateTime.UtcNow;
            if (lastStateUpdated == null
                || (currentTimeStamp - lastStateUpdated.Value).TotalMilliseconds >= StateRepositoryUpdateInterval)
            {
                isUpdated = true;
                var isDeadlockDetected = (currentTimeStamp - lastReported_).TotalMilliseconds >= DeadlockTimeout;
                if (isDeadlockDetected)
                {
                    deadlockCts?.Cancel();
                }

                await stateRepository_.UpdateState(tableId, tableType, startTime, endTime, isDeadlockDetected, statistics.ToArray());
            }

            return isUpdated ? currentTimeStamp : lastStateUpdated;
        }

        private static void AggregateStats(IDictionary<int, TotalStats> statistics, Stats stats)
        {
            var pickUpForksTime = stats.PickUpForksTime;
            var putDownForksTime = stats.PutDownForksTime;
            var eatTime = stats.EatSpaghettiTime;
            var thinkTime = stats.ThinkTime;

            if (!statistics.TryGetValue(stats.PhilosopherId, out TotalStats sp))
            {
                sp = new TotalStats();
                statistics[stats.PhilosopherId] = sp;
            }

            sp.PickUpForksTime += pickUpForksTime;
            sp.PutDownForksTime += putDownForksTime;
            sp.EatSpaghettiTime += eatTime;
            sp.ThinkTime += thinkTime;
            sp.TotalTime += stats.FullTime;
            sp.TotalCycles += 1;
        }
    }
}
