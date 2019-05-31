using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace DiningPhilosophers.WebApi.Services
{
    public class RedisConnectionService : IRedisConnectionService, IDisposable
    {
        private readonly IConfigurationService configurationService_;
        private readonly object sync_ = new object();
        private volatile Lazy<ConnectionMultiplexer> connection_;

        public RedisConnectionService(IConfigurationService configurationService)
        {
            configurationService_ = configurationService;
            Initialize();
        }

        public Task<T> Execute<T>(Func<IDatabase, Task<T>> dbFunc, int retryCount)
        {
            return Execute(() =>
            {
                var db = GetDatabase();
                return dbFunc(db);
            }, retryCount);
        }

        public Task Publish(string channelName, string[] messages, int retryCount)
        {
            var channel = new RedisChannel(channelName, RedisChannel.PatternMode.Literal);

            return Execute(async () =>
            {
                var subscriber = GetSubscriber();
                foreach (var message in messages)
                {
                    await subscriber.PublishAsync(channel, message);
                }

                return (object) null;

            }, retryCount);
        }

        public IObservable<string> GetStream(string channelName, int retryCount)
        {
            var channel = new RedisChannel(channelName, RedisChannel.PatternMode.Literal);
            
            return Observable.Create<string>(async observer =>
            {
                void Handler(RedisChannel redisChannel, RedisValue redisValue)
                {
                    var msgStr = (string) redisValue;
                    observer.OnNext(msgStr);
                }

                await Execute(async () =>
                {
                    var subscriber = GetSubscriber();
                    await subscriber.SubscribeAsync(channel, Handler);
                    return (object)null;
                }, retryCount);

                return () =>
                {
                    Execute(() =>
                        {
                            var subscriber = GetSubscriber();
                            subscriber.Unsubscribe(channel, Handler);
                            return Task.FromResult((object) null);
                        }, retryCount)
                        .Wait();
                };
            });
        }

        public void Dispose()
        {
            DisposeConnection();
        }

        private Task<T> Execute<T>(Func<Task<T>> redisFunc, int retryCount)
        {
            try
            {
                return redisFunc();
            }
            catch (RedisConnectionException)
            {
                if (retryCount == 0)
                {
                    throw;
                }

                Thread.Sleep(100);
                Initialize();
                return Execute(redisFunc, retryCount - 1);
            }
            catch (ObjectDisposedException)
            {
                if (retryCount == 0)
                {
                    throw;
                }

                Thread.Sleep(100);
                return Execute(redisFunc, retryCount - 1);
            }
            catch (RedisTimeoutException)
            {
                if (retryCount == 0)
                {
                    throw;
                }

                Thread.Sleep(100);
                return Execute(redisFunc, retryCount - 1);
            }
        }

        private IDatabase GetDatabase()
        {
            IDatabase databaseValue;
            lock (sync_)
            {
                databaseValue = connection_.Value.GetDatabase();
            }

            return databaseValue;
        }

        private ISubscriber GetSubscriber()
        {
            ISubscriber subscriber;
            lock (sync_)
            {
                subscriber = connection_.Value.GetSubscriber();
            }

            return subscriber;
        }

        private void DisposeConnection()
        {
            lock (sync_)
            {
                if (connection_ != null && connection_.IsValueCreated)
                {
                    connection_.Value.Dispose();
                }
            }
        }

        private void Initialize()
        {
            lock (sync_)
            {
                DisposeConnection();

                connection_ = new Lazy<ConnectionMultiplexer>(
                    () =>
                    {
                        var connectionMultiplexer = ConnectionMultiplexer.Connect(ConfigurationOptions.Parse(configurationService_.RedisHost));
                        return connectionMultiplexer;
                    },
                    LazyThreadSafetyMode.ExecutionAndPublication);
            }
        }
    }
}