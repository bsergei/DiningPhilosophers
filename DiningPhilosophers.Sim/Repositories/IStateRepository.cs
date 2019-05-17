using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DiningPhilosophers.Core.Model;
using DiningPhilosophers.Sim.Model;

namespace DiningPhilosophers.Sim.Repositories
{
    public interface IStateRepository
    {
        Task<StateDto[]> Get();

        Task<StateDto[]> Get(Guid tableId);

        Task UpdateState(
            Guid tableId,
            TableType tableType,
            DateTime startTime, 
            DateTime? endTime, 
            bool isDeadlockDetected,
            KeyValuePair<int, TotalStats>[] stats);
    }
}
