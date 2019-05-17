using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DiningPhilosophers.Sim.Model;
using DiningPhilosophers.Sim.Services;
using DiningPhilosophers.WebApi.Model;
using Microsoft.Extensions.Hosting;

namespace DiningPhilosophers.WebApi.Services
{
    public class DistributedRunnerService : IRunnerService, IHostedService
    {
        private readonly ITableService tableService_;
        private readonly IDistributedPubSubService pubSubService_;
        private readonly ConcurrentDictionary<Guid, Tuple<Task, CancellationTokenSource>> runningTables_;

        private IDisposable stopRequestsSubscription_;

        public DistributedRunnerService(
            ITableService tableService,
            IDistributedPubSubService pubSubService)
        {
            tableService_ = tableService;
            pubSubService_ = pubSubService;
            runningTables_ = new ConcurrentDictionary<Guid, Tuple<Task, CancellationTokenSource>>();
        }

        public Guid Start(TableType tableType, int philosophersCount, CancellationToken cancellationToken, Func<Task> onFinish)
        {
            Guid tableId = Guid.NewGuid();
            var jobStopCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            Task task =
                Task.Run(() => tableService_.Run(tableId, tableType, philosophersCount, jobStopCts.Token))
                    .ContinueWith(t => runningTables_.TryRemove(tableId, out Tuple<Task, CancellationTokenSource> _))
                    .ContinueWith(t => jobStopCts.Dispose())
                    .ContinueWith(t => onFinish?.Invoke());

            runningTables_[tableId] = Tuple.Create(task, jobStopCts);
            return tableId;
        }

        public async Task Stop(Guid tableId)
        {
            // Notify servers in the swarm to stop this tableId.
            // Each RunnerService of the each running server in the swarm subscribes to this channel.
            // This mechanism required because we don't know which server will execute Stop(tableId) request.
            // I.e., we notify all servers in swarm to try handle it.
            await pubSubService_.Publish(new TableStopRequest { TableId = tableId });
        }

        #region IHostedService Implementation

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Subscribe to TableStopRequest channel.
            stopRequestsSubscription_ = pubSubService_.Subscribe<TableStopRequest>(stopReq => StopInternal(stopReq.TableId));
            return Task.CompletedTask;
        }

        /// <summary>
        /// Implements graceful stop when host is stopped.
        /// </summary>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            // Unsubscribe from TableStopRequest channel.
            stopRequestsSubscription_?.Dispose();

            // Stop all jobs that this server started.
            var tableIds = runningTables_.Keys.ToArray();
            var tasks = new Task[tableIds.Length];
            for (var index = 0; index < tableIds.Length; index++)
            {
                var tableId = tableIds[index];
                tasks[index] = StopInternal(tableId);
            }

            await Task.WhenAny(
                Task.WhenAll(tasks),
                Task.Delay(Timeout.Infinite, cancellationToken));
        }

        #endregion

        private async Task<bool> StopInternal(Guid tableId)
        {
            if (runningTables_.TryRemove(tableId, out Tuple<Task, CancellationTokenSource> cts))
            {
                cts.Item2.Cancel();
                await cts.Item1;
                return true;
            }

            return false;
        }
    }
}