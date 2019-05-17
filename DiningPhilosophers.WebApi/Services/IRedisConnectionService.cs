using System;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace DiningPhilosophers.WebApi.Services
{
    public interface IRedisConnectionService
    {
        Task<T> Execute<T>(Func<IDatabase, Task<T>> dbFunc, int retryCount = 10);

        Task Publish(string channelName, string[] messages, int retryCount = 10);

        IObservable<string> GetStream(string channelName, int retryCount = 10);
    }
}