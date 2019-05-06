using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using DiningPhilosophers.Core;
using DiningPhilosophers.Core.Model;

namespace DiningPhilosophers.Log
{
    /// <summary>
    /// Helper class to track and log Philosophers statistics and detect dead-locks.
    /// </summary>
    public class SimulationLog : IProgress<Stats>
    {
        private const int UpdateTimeoutMilliseconds = 1000;
        private const int DeadlockTimeoutMilliseconds = 500;

        private readonly Philosopher[] philosophers_;
        private readonly int totalSimulationSeconds_;
        private readonly BufferedProducerConsumer<Stats> statsBuffer_ = new BufferedProducerConsumer<Stats>();
        private readonly Dictionary<int, TotalStats> statistics_ = new Dictionary<int, TotalStats>();
        private readonly Task[] tasks_;

        private DateTime lastUpdated_ = DateTime.UtcNow; // Used to detect deadlocks.

        public SimulationLog(Philosopher[] philosophers, CancellationToken cancellationToken,
            int totalSimulationSeconds)
        {
            philosophers_ = philosophers;
            totalSimulationSeconds_ = totalSimulationSeconds;
            SimulationTask = TaskProc(cancellationToken);
            tasks_ = new[]
            {
                SimulationTask,
                StatsProc()
            };
        }

        public Task SimulationTask { get; private set; }

        public void Finish()
        {
            Console.WriteLine($"Requested FINISH.");

            statsBuffer_.Finish();
            if (tasks_ != null)
            {
                foreach (var task in tasks_)
                {
                    try
                    {
                        task.Wait();
                    }
                    catch (AggregateException e)
                    {
                        if (e.InnerExceptions.Count == 1
                            && e.InnerExceptions[0] is TaskCanceledException)
                        {
                            continue;
                        }

                        throw;
                    }
                    catch (TaskCanceledException)
                    {
                    }
                }
            }
        }

        public void Report(Stats value)
        {
            statsBuffer_.Push(value);
            lastUpdated_ = DateTime.UtcNow;
        }

        private async Task StatsProc()
        {
            while (await statsBuffer_.Source.OutputAvailableAsync())
            {
                Stats[] data = statsBuffer_.Source.Receive();

                foreach (var s in data)
                {
                    if (!statistics_.TryGetValue(s.PhilosopherId, out TotalStats sp))
                    {
                        sp = new TotalStats();
                        statistics_[s.PhilosopherId] = sp;
                    }

                    var pickUpForksTime = s.PickUpForksTime;
                    var putDownForksTime = s.PutDownForksTime;
                    var eatTime = s.EatSpaghettiTime;
                    var thinkTime = s.ThinkTime;

                    sp.PickUpForksTime += pickUpForksTime;
                    sp.PutDownForksTime += putDownForksTime;
                    sp.EatSpaghettiTime += eatTime;
                    sp.ThinkTime += thinkTime;
                    sp.TotalTime += s.FullTime;
                    sp.TotalCycles += 1;
                }
            }
        }

        public void DumpStatistics()
        {
            var ultimateTotal = new TotalStats();

            foreach (var t in statistics_.Values)
            {
                ultimateTotal.PickUpForksTime += t.PickUpForksTime;
                ultimateTotal.PutDownForksTime += t.PutDownForksTime;
                ultimateTotal.EatSpaghettiTime += t.EatSpaghettiTime;
                ultimateTotal.ThinkTime += t.ThinkTime;
                ultimateTotal.TotalTime += t.TotalTime;
                ultimateTotal.TotalCycles += t.TotalCycles;

                if (t.TotalTime == 0)
                {
                    t.TotalTime = Int64.MaxValue;
                }
            }

            if (ultimateTotal.TotalTime == 0)
            {
                ultimateTotal.TotalTime = Int64.MaxValue;
            }

            Console.WriteLine($"Stats:");
            foreach (var p in philosophers_)
            {
                statistics_.TryGetValue(p.Id, out TotalStats stat);
                Console.WriteLine($"Philosopher #{p.Id.ToString().PadLeft(2)}, {stat}");
            }

            Console.WriteLine($"TOTAL: {ultimateTotal}");

            foreach (var p in philosophers_)
            {
                statistics_.TryGetValue(p.Id, out TotalStats stat);
                Console.WriteLine(
                    $"{p.Id}\t{1.0m * stat.PickUpForksTime / stat.TotalTime}\t{1.0m * stat.EatSpaghettiTime / stat.TotalTime}\t{1.0m * stat.PutDownForksTime / stat.TotalTime}\t{1.0m * stat.ThinkTime / stat.TotalTime}");
            }
        }

        private async Task TaskProc(CancellationToken ct)
        {
            DateTime lastTime = DateTime.UtcNow;
            DateTime start = lastTime;
            bool isDeadlockDetected = false;
            while (!ct.IsCancellationRequested && !isDeadlockDetected)
            {
                var now = DateTime.UtcNow;

                // Assumes that deadlock occurred when nobody
                // reports progress since DeadlockTimeoutMilliseconds
                isDeadlockDetected = (now - lastUpdated_).TotalMilliseconds > DeadlockTimeoutMilliseconds;

                var elapsed = now - lastTime;
                if (elapsed.TotalMilliseconds > UpdateTimeoutMilliseconds)
                {
                    lastTime = now;
                    double totalSecondsPassed = (now - start).TotalSeconds;
                    double progressPrc =
                        Math.Min(totalSecondsPassed, totalSimulationSeconds_) / totalSimulationSeconds_;
                    Console.WriteLine($"Simulating {totalSecondsPassed:0.0} seconds ({progressPrc:P0}).");
                }

                await Task.Delay(UpdateTimeoutMilliseconds / 100, ct);
            }

            if (isDeadlockDetected)
            {
                Console.WriteLine("!!! Deadlock detected.");
            }
        }
    }
}