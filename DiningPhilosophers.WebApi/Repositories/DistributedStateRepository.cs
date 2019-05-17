using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DiningPhilosophers.Core.Model;
using DiningPhilosophers.Sim.Model;
using DiningPhilosophers.Sim.Repositories;
using DiningPhilosophers.WebApi.Services;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace DiningPhilosophers.WebApi.Repositories
{
    public class DistributedStateRepository : IStateRepository
    {
        private const string DataKey = "DiningPhilosopherState:{0}:{1}:{2}";
        private const string TableIdPattern = "DiningPhilosopherState:*:{0}:*";
        private const string AllPattern = "DiningPhilosopherState:*";

        private readonly IRedisConnectionService redisConnectionService_;
        private readonly IDistributedPubSubService pubSubService_;

        public DistributedStateRepository(
            IRedisConnectionService redisConnectionService,
            IDistributedPubSubService pubSubService)
        {
            redisConnectionService_ = redisConnectionService;
            pubSubService_ = pubSubService;
        }

        public async Task<StateDto[]> Get()
        {
            var keys = await SearchKeys(AllPattern);
            return await redisConnectionService_.Execute(async db =>
            {
                RedisValue[] redisValues = await db.StringGetAsync(keys);
                return redisValues.Select(v => JsonConvert.DeserializeObject<StateDto>((string)v)).ToArray();
            });
        }

        public async Task<StateDto[]> Get(Guid tableId)
        {
            var keys = await SearchKeys(String.Format(TableIdPattern, tableId));
            return await redisConnectionService_.Execute(async db =>
            {
                RedisValue[] redisValues = await db.StringGetAsync(keys);
                return redisValues.Select(v => JsonConvert.DeserializeObject<StateDto>((string)v)).ToArray();
            });
        }

        public async Task UpdateState(
            Guid tableId,
            TableType tableType, 
            DateTime startTime, 
            DateTime? endTime,
            bool isDeadlockDetected, 
            KeyValuePair<int, TotalStats>[] stats)
        {
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

            var data =
                stateDtos.Select(_ =>
                        new KeyValuePair<RedisKey, RedisValue>(
                            String.Format(DataKey, _.TableType, tableId, _.PhilosopherId),
                            JsonConvert.SerializeObject(_)))
                    .ToArray();

            await redisConnectionService_.Execute(async db =>
            {
                var result = await db.StringSetAsync(data);
                foreach (var kvp in data)
                {
                    db.KeyExpire(kvp.Key, TimeSpan.FromHours(1), CommandFlags.FireAndForget);
                }
                return result;
            });
            await pubSubService_.Publish(stateDtos);
        }

        private Task<RedisKey[]> SearchKeys(string pattern)
        {
            return redisConnectionService_.Execute(async db =>
            {
                var keys = new HashSet<string>();
                long nextCursor = 0L;
                do
                {
                    RedisResult execResult = await db.ExecuteAsync("SCAN", nextCursor.ToString(), "MATCH", pattern, "COUNT", "1000");
                    RedisResult[] scanResult = (RedisResult[])execResult;

                    var resultLines = ((string[])scanResult[1]);
                    keys.UnionWith(resultLines);

                    nextCursor = Int64.Parse((string)scanResult[0]);
                }
                while (nextCursor != 0L);

                return keys.Select(_ => (RedisKey) _).ToArray();
            });
        }
    }
}