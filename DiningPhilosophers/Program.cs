using System;
using System.Threading;
using DiningPhilosophers.Core;
using DiningPhilosophers.Factories;
using DiningPhilosophers.Log;
using DiningPhilosophers.Model;

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
            IDomainFactory philosopherFactory = ChoicePhilosopherFactory();
            if (philosopherFactory == null)
            {
                return;
            }

            // Create gang of Philosophers.
            var cancellationTokenSource = new CancellationTokenSource();
            Philosopher[] philosophers = philosopherFactory.CreateDiningPhilosophers(PhilosophersCount);
            var simulationLog = new SimulationLog(philosophers, cancellationTokenSource.Token, SimulationTimeSeconds);
            
            // Run Philosophers.
            foreach (Philosopher philosopher in philosophers)
            {
                var runner = new PhilosopherRunner(philosopher);
                runner.Start(cancellationTokenSource.Token, simulationLog);
            }

            // Log and wait necessary simulation time.
            Simulate(cancellationTokenSource, simulationLog);
        }

        private static IDomainFactory ChoicePhilosopherFactory()
        {
            IDomainFactory factory = null;

            Console.WriteLine("Type 0 to exit");
            Console.WriteLine("---------------------------------");
            Console.WriteLine("Type 1 for Problem");
            Console.WriteLine("Type 2 for Dijkstra");
            Console.WriteLine("Type 3 for Arbitrary");
            Console.WriteLine("Type 4 for Agile");
            Console.WriteLine("Type 5 for Chandy-Misra");

            bool isExit = false;
            while (factory == null && !isExit)
            {
                var key = Console.ReadKey();
                switch (key.KeyChar)
                {
                    case '0':
                        isExit = true;
                        break;

                    case '1':
                        factory = new ProblemFactory();
                        break;

                    case '2':
                        factory = new DijkstraFactory();
                        break;

                    case '3':
                        factory = new ArbitraryFactory();
                        break;

                    case '4':
                        factory = new AgileFactory(); 
                        break;

                    case '5':
                        factory = new ChandyMisraFactory();
                        break;
                }
            }
            Console.WriteLine();
            return factory;
        }

        private static void Simulate(
            CancellationTokenSource cancellationTokenSource, 
            SimulationLog simulationLog)
        {
            bool finished = false;
            void Exit()
            {
                if (finished)
                {
                    return;
                }

                finished = true;

                cancellationTokenSource.Cancel();
                simulationLog.Finish();
                simulationLog.DumpStatistics();

                Environment.Exit(0);
            }

            void WaitAndExit()
            {
                // Wait SimulationTimeSeconds or until simulationLog.SimulationTask finished then exit.
                simulationLog.SimulationTask.Wait(TimeSpan.FromSeconds(SimulationTimeSeconds));
                Exit();
            }

            // Calls WaitAndExit in separate thread with highest priority.
            Thread simulationWatcherThread = new Thread(WaitAndExit)
            {
                Priority = ThreadPriority.Highest,
                IsBackground = true
            };
            simulationWatcherThread.Start();
            
            Console.ReadLine();
            Exit();
        }
    }
}