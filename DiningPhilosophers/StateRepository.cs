using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DiningPhilosophers.Core.Model;
using DiningPhilosophers.Sim.Model;
using DiningPhilosophers.Sim.Repositories;

namespace DiningPhilosophers
{
    public class StateRepository : IStateRepository
    {
        private readonly ConcurrentDictionary<Tuple<Guid, int>, StateDto> stateDtos_ = new ConcurrentDictionary<Tuple<Guid, int>, StateDto>();
        private DateTime lastUpdated_ = DateTime.MinValue;

        public Task<StateDto[]> Get()
        {
            var stateDtos = stateDtos_
                .Select(_ => _.Value)
                .ToArray();

            return Task.FromResult(stateDtos);
        }

        public Task<StateDto[]> Get(Guid tableId)
        {
            var stateDtos = stateDtos_
                .Where(_ => _.Key.Item1 == tableId)
                .Select(_ => _.Value)
                .ToArray();

            return Task.FromResult(stateDtos);
        }

        public Task UpdateState(
            Guid tableId, 
            TableType tableType, 
            DateTime startTime, 
            DateTime? endTime, 
            bool isDeadlockDetected,
            KeyValuePair<int, TotalStats>[] stats)
        {
            if ((DateTime.UtcNow - lastUpdated_).TotalMilliseconds >= 1000)
            {
                Console.WriteLine($"Simulating { (DateTime.UtcNow - startTime).TotalSeconds } seconds...");
            }

            lastUpdated_ = DateTime.UtcNow;

            var stateDtos = stats.Select(_ => new StateDto
                {
                    TableId = tableId,
                    TableType = tableType,
                    PhilosopherId = _.Key,
                    StartTime = startTime,
                    EndTime = endTime,
                    TotalStats = _.Value,
                    IsDeadlockDetected = isDeadlockDetected,
                })
                .ToArray();

            foreach (var stateDto in stateDtos)
            {
                stateDtos_[Tuple.Create(stateDto.TableId, stateDto.PhilosopherId)] = stateDto;
            }

            return Task.CompletedTask;
        }
    }
}