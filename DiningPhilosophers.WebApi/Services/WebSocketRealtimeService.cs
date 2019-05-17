using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace DiningPhilosophers.WebApi.Services
{
    public class WebSocketRealtimeService<T> : IWebSocketRealtimeService<T>
    {
        private const int ClientPingTimeoutSeconds = 10;
        private const string ClientPingMessage = "PING";

        private readonly IDistributedPubSubService distributedPubSubService_;
        private readonly JsonSerializerSettings jsonOptions_;

        public WebSocketRealtimeService(
            IDistributedPubSubService distributedPubSubService,
            IOptions<MvcJsonOptions> jsonOptions)
        {
            distributedPubSubService_ = distributedPubSubService;
            jsonOptions_ = jsonOptions.Value.SerializerSettings;
        }

        public async Task Run(WebSocket webSocket, Func<T, bool> filter, CancellationToken cancellationToken)
        {
            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                var listenClientTask = ListenClient(webSocket, cts.Token);
                var sendToClientTask = WebSocketSend(webSocket, filter, cts.Token);

                await Task.WhenAny(
                    listenClientTask,
                    sendToClientTask);

                cts.Cancel(); // Cancel any other task.

                var result = listenClientTask.Result;

                if (webSocket.State == WebSocketState.Open 
                    || webSocket.State == WebSocketState.CloseReceived)
                {
                    await webSocket.CloseAsync(
                        result == null
                            ? WebSocketCloseStatus.NormalClosure
                            : WebSocketCloseStatus.ProtocolError,
                        result ?? String.Empty,
                        CancellationToken.None);
                }
            }
        }

        private async Task WebSocketSend(WebSocket webSocket, Func<T, bool> filter, CancellationToken cancellationToken)
        {
            var collection = new BufferBlock<T>();
            using (cancellationToken.Register(() => collection.Complete())) // Complete blocking collection on cancel.
            {
                // Subscribe on pub/sub channel and push incoming values into collection.
                using (distributedPubSubService_.Subscribe(collection)) 
                {
                    try
                    {
                        await WebSocketSend(webSocket, filter, collection, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                }
            }
        }

        private async Task<string> ListenClient(
            WebSocket webSocket, 
            CancellationToken cancellationToken)
        {
            var buffer = new byte[1024];
            while (webSocket.State == WebSocketState.Open || webSocket.State == WebSocketState.CloseSent)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return null;
                }

                using (var pingTimeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(ClientPingTimeoutSeconds)))
                {
                    using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, pingTimeoutCts.Token))
                    {
                        WebSocketReceiveResult result;
                        var arraySegment = new ArraySegment<byte>(buffer);
                        try
                        {
                            result = await webSocket.ReceiveAsync(arraySegment, linkedCts.Token);
                        }
                        catch (OperationCanceledException)
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                // Normal closure, ignore ping timeout.
                                return null;
                            }
                            else
                            {
                                return "No PING received from client";
                            }
                        }

                        switch (result.MessageType)
                        {
                            case WebSocketMessageType.Close:
                                // Client sent normal close.
                                return null;

                            case WebSocketMessageType.Text:
                                var receivedString = Encoding.ASCII.GetString(arraySegment.Array, 0, result.Count);
                                if (!String.Equals(ClientPingMessage, receivedString,
                                    StringComparison.InvariantCultureIgnoreCase))
                                {
                                    // Not a ping.
                                    return "Only PING messages accepted from client";
                                }

                                break;

                            case WebSocketMessageType.Binary:
                                return "Only text PING messages accepted from client";
                        }
                    }
                }
            }

            return null;
        }

        private async Task WebSocketSend(WebSocket webSocket, Func<T, bool> filter, IReceivableSourceBlock<T> src, CancellationToken cancellationToken)
        {
            while (await src.OutputAvailableAsync(cancellationToken) &&
                   webSocket.State == WebSocketState.Open)
            {
                while (src.TryReceiveAll(out var itemsArray))
                {
                    foreach (var item in itemsArray)
                    {
                        if (filter?.Invoke(item) ?? true)
                        {
                            var msgStr = JsonConvert.SerializeObject(item, jsonOptions_);
                            var bytes = Encoding.UTF8.GetBytes(msgStr);
                            var arraySegment = new ArraySegment<byte>(bytes);
                            await webSocket.SendAsync(
                                arraySegment,
                                WebSocketMessageType.Text,
                                true,
                                cancellationToken);
                        }
                    }
                }
            }
        }
    }
}