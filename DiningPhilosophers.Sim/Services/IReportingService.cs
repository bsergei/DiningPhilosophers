using System;
using System.Threading;
using System.Threading.Tasks;
using DiningPhilosophers.Core.Model;
using DiningPhilosophers.Sim.Model;

namespace DiningPhilosophers.Sim.Services
{
    /// <summary>
    /// Service that implements IProgress and process philosophers statistics.
    /// </summary>
    public interface IReportingService : IProgress<Stats>
    {
        /// <summary>
        /// Starts new task that is processing incoming stats from philosophers.
        /// </summary>
        Task ReceiveStats(Guid tableId, TableType tableType, CancellationToken ct);
    }
}