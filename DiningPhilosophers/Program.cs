using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DiningPhilosophers.Core.Model;
using DiningPhilosophers.Sim.Model;
using DiningPhilosophers.Sim.Repositories;
using DiningPhilosophers.Sim.Services;

namespace DiningPhilosophers
{
    class Program
    {
        /// <summary>
        /// Number of Philosophers at the table.
        /// </summary>
        private const int PhilosophersCount = 30;

        /// <summary>
        /// Simulation time in seconds.
        /// </summary>
        private const int SimulationTimeSeconds = 10;

        public static void Main(string[] args)
        {
            // Choice factory.
            var tableType = ChoiceTable();
            if (tableType == null)
            {
                return;
            }

            Simulate(tableType.Value, PhilosophersCount, SimulationTimeSeconds);
        }

        private static void Simulate(TableType tableType, int count, int seconds)
        {
            Console.WriteLine($"Simulating {tableType}... Press ENTER to exit.");
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                var task = RunSimulation(tableType, count, seconds, cts.Token);

                Console.ReadLine();
                cts.Cancel();

                task.Wait(CancellationToken.None);
            }
        }

        private static async Task RunSimulation(TableType tableType, int count, int seconds, CancellationToken cancellationToken)
        {
            using (var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(seconds)))
            {
                using (var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, cancellationToken))
                {
                    var tableId = Guid.NewGuid();
                    IStateRepository stateRepository = new StateRepository();
                    ITableService tableService = new TableService(() => new ReportingService(stateRepository), TableTypeExtensions.CreateDomainFactory);
                    
                    await tableService.Run(tableId, tableType, count, cancellationTokenSource.Token);

                    StateDto[] stateDtos = await stateRepository.Get(tableId);
                    PrintResult(stateDtos);
                }
            }
        }

        private static void PrintResult(StateDto[] stateDtos)
        {
            if (stateDtos.Any(_ => _.IsDeadlockDetected))
            {
                Console.WriteLine("!!! DEADLOCK detected.");
            }

            Console.WriteLine($"Stats:");

            var ultimateTotal = new TotalStats();

            foreach (var p in stateDtos.OrderBy(_ => _.PhilosopherId))
            {
                var t = p.TotalStats;

                Console.WriteLine($"Philosopher #{p.PhilosopherId.ToString().PadLeft(2)}, {t}");

                ultimateTotal.PickUpForksTime += t.PickUpForksTime;
                ultimateTotal.PutDownForksTime += t.PutDownForksTime;
                ultimateTotal.EatSpaghettiTime += t.EatSpaghettiTime;
                ultimateTotal.ThinkTime += t.ThinkTime;
                ultimateTotal.TotalTime += t.TotalTime;
                ultimateTotal.TotalCycles += t.TotalCycles;
            }

            Console.WriteLine($"TOTAL: {ultimateTotal}");
        }

        private static TableType? ChoiceTable()
        {
            TableType? tableType = null;

            Console.WriteLine("Type 0 to exit");
            Console.WriteLine("---------------------------------");
            Console.WriteLine("Type 1 for Problem");
            Console.WriteLine("Type 2 for Dijkstra");
            Console.WriteLine("Type 3 for Arbitrary");
            Console.WriteLine("Type 4 for Agile");
            Console.WriteLine("Type 5 for Chandy-Misra");

            bool isExit = false;
            while (tableType == null && !isExit)
            {
                var key = Console.ReadKey();
                switch (key.KeyChar)
                {
                    case '0':
                        isExit = true;
                        break;

                    case '1':
                        tableType = TableType.Problem;
                        break;

                    case '2':
                        tableType = TableType.Dijkstra;
                        break;

                    case '3':
                        tableType = TableType.Arbitrary;
                        break;

                    case '4':
                        tableType = TableType.Agile;
                        break;

                    case '5':
                        tableType = TableType.ChandyMisra;
                        break;
                }
            }

            Console.WriteLine();
            return tableType;
        }
    }
}