using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace DiningPhilosophers.WebApi.Services
{
    public interface IWebSocketRealtimeService<T>
    {
        Task Run(WebSocket webSocket, Func<T, bool> filter, CancellationToken cancellationToken);
    }
}