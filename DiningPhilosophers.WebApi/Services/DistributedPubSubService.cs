using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DiningPhilosophers.WebApi.Services
{
    public class DistributedPubSubService : IDistributedPubSubService
    {
        private readonly IRedisConnectionService redisConnectionService_;

        public DistributedPubSubService(IRedisConnectionService redisConnectionService)
        {
            redisConnectionService_ = redisConnectionService;
        }

        public IDisposable Subscribe<T>(Action<T> onNext)
        {
            return redisConnectionService_
                .GetStream(GetChannelName<T>())
                .Select(JsonConvert.DeserializeObject<T>)
                .Subscribe(onNext);
        }

        public Task Publish<T>(T[] values)
        {
            return redisConnectionService_.Publish(
                GetChannelName<T>(),
                values
                    .Select(_ => JsonConvert.SerializeObject(_))
                    .ToArray());
        }

        private static string GetChannelName<T>()
        {
            return typeof(T).FullName;
        }
    }
}