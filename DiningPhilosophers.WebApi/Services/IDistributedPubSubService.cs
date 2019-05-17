using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace DiningPhilosophers.WebApi.Services
{
    public interface IDistributedPubSubService
    {
        IDisposable Subscribe<T>(Action<T> onNext);

        Task Publish<T>(params T[] values);
    }

    public static class ObjectPubSubServiceExtensions
    {
        public static IDisposable Subscribe<T>(this IDistributedPubSubService distributedPubSubService, ITargetBlock<T> target)
        {
            return distributedPubSubService.Subscribe<T>(item => target.Post(item));
        }
    }
}