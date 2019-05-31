using System;

namespace DiningPhilosophers.WebApi.Services
{
    public class EnvConfigurationService : IConfigurationService
    {
        public string RedisHost
        {
            get
            {
                var envValue = Environment.GetEnvironmentVariable("RedisHost");
                return String.IsNullOrWhiteSpace(envValue) ? "localhost:6379" : envValue;
            }
        }
    }
}