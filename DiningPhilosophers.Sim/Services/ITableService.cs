using System;
using System.Threading;
using System.Threading.Tasks;
using DiningPhilosophers.Sim.Model;

namespace DiningPhilosophers.Sim.Services
{
    public interface ITableService
    {
        Task Run(Guid tableId, TableType tableType, int count, CancellationToken cancellationToken);
    }
}