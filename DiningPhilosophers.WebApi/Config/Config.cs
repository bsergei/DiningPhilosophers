using System;
using DiningPhilosophers.Core.Factories;
using DiningPhilosophers.Sim.Model;
using DiningPhilosophers.Sim.Repositories;
using DiningPhilosophers.Sim.Services;
using DiningPhilosophers.WebApi.Repositories;
using DiningPhilosophers.WebApi.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DiningPhilosophers.WebApi.Config
{
    public static class Config
    {
        public static void AddDiningPhilosophersDomain(this IServiceCollection services)
        {
            services.AddSingleton<IRedisConnectionService, RedisConnectionService>();
            services.AddTransient<IDistributedPubSubService, DistributedPubSubService>();
            services.AddTransient(typeof(IWebSocketRealtimeService<>), typeof(WebSocketRealtimeService<>));
            
            services.AddSingleton<DistributedRunnerService>();
            services.AddSingleton<IRunnerService>(p => p.GetRequiredService<DistributedRunnerService>());
            services.AddSingleton<IHostedService>(p => p.GetRequiredService<DistributedRunnerService>());

            services.AddTransient<IStateRepository, DistributedStateRepository>();
            services.AddTransient<IReportingService, ReportingService>();
            services.AddTransient<ITableService, TableService>();
            services.AddTransient<Func<IReportingService>>(provider => provider.GetRequiredService<IReportingService>);
            services.AddTransient<Func<TableType, IDomainFactory>>(p => tableType => tableType.CreateDomainFactory());
        }
    }
}
