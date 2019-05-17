using System;
using System.Threading;
using System.Threading.Tasks;
using DiningPhilosophers.Sim.Model;

namespace DiningPhilosophers.WebApi.Services
{
    public interface IRunnerService
    {
        /// <summary>
        /// Starts new job for specified table type and philosophers count.
        /// Returns unique identifier of the table id.
        /// </summary>
        Guid Start(TableType tableType, int philosophersCount, CancellationToken ct, Func<Task> onFinish = null);

        /// <summary>
        /// Stops table by its ID and returns true if job was running.
        /// </summary>
        Task Stop(Guid tableId);
    }
}