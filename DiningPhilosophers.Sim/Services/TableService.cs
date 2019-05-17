using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DiningPhilosophers.Core.Factories;
using DiningPhilosophers.Core.Model;
using DiningPhilosophers.Core.Model.Hands;
using DiningPhilosophers.Core.Utils;
using DiningPhilosophers.Sim.Model;

namespace DiningPhilosophers.Sim.Services
{
    public class TableService : ITableService
    {
        private readonly Func<IReportingService> reportingServiceFactory_;
        private readonly Func<TableType, IDomainFactory> domainFactory_;

        public TableService(Func<IReportingService> reportingServiceFactory, Func<TableType, IDomainFactory> domainFactory)
        {
            reportingServiceFactory_ = reportingServiceFactory;
            domainFactory_ = domainFactory;
        }

        public async Task Run(Guid tableId, TableType tableType, int count, CancellationToken cancellationToken)
        {
            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                IReportingService reportingService = reportingServiceFactory_();

                Task philosophersTask = RunPhilosophers(tableType, count, reportingService, cts.Token);
                Task reportingTask = reportingService.ReceiveStats(tableId, tableType, cts.Token);

                try
                {
                    await Task.WhenAny(
                        philosophersTask,
                        reportingTask);
                }
                finally
                {
                    // Cancel any other task.
                    cts.Cancel();
                }
            }
        }

        private Task RunPhilosophers(TableType tableType, int count, IProgress<Stats> progress, CancellationToken cancellationToken)
        {
            var exceptions = new List<Exception>();
            var threads = new List<Thread>();
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            PhilosopherRunner[] runners = CreatePhilosopherRunners(tableType, count);
            foreach (var philosopherRunner in runners)
            {
                Thread thread = philosopherRunner.Start(
                    cts.Token,
                    progress,
                    PhilosopherRunnerErrorHandler(exceptions, cts));

                threads.Add(thread);
            }

            return Task.Run(() =>
            {
                foreach (var thread in threads)
                {
                    thread.Join();
                }

                cts.Dispose();

                if (exceptions.Count > 0)
                {
                    throw new AggregateException(exceptions);
                }
            });
        }

        private PhilosopherRunner[] CreatePhilosopherRunners(TableType tableType, int count)
        {
            IDomainFactory domainFactory = domainFactory_(tableType);
            Philosopher[] philosophers = domainFactory.CreateDiningPhilosophers(count);
            PhilosopherRunner[] runners = philosophers.Select(_ => new PhilosopherRunner(_)).ToArray();
            return runners;
        }

        private Action<Exception> PhilosopherRunnerErrorHandler(List<Exception> list, CancellationTokenSource cts)
        {
            return exception =>
            {
                lock (list)
                {
                    // Ignore DeadlockDetectedException as we have additional mechanism to detect deadlock.
                    // DeadlockDetectedException used primarily to release resources.
                    if (!(exception is DeadlockDetectedException)) 
                    {
                        list.Add(exception);
                    }
                }

                cts.Cancel();
            };
        }
    }
}