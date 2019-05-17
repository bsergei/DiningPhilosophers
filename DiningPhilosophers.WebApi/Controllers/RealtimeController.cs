using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using DiningPhilosophers.Sim.Model;
using DiningPhilosophers.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace DiningPhilosophers.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class RealtimeController : ControllerBase
    {
        private readonly IWebSocketRealtimeService<StateDto> stateDtoRealtimeService_;

        public RealtimeController(IWebSocketRealtimeService<StateDto> stateDtoRealtimeService)
        {
            stateDtoRealtimeService_ = stateDtoRealtimeService;
        }

        [HttpGet]
        public async Task Get([FromQuery] Guid? tableId = null)
        {
            var webSockets = ControllerContext.HttpContext.WebSockets;
            if (webSockets.IsWebSocketRequest)
            {
                using (WebSocket webSocket = await webSockets.AcceptWebSocketAsync())
                {
                    var filter = tableId == null 
                        ? (Func<StateDto, bool>)null 
                        : stateDto => stateDto.TableId == tableId.Value;
                    await stateDtoRealtimeService_.Run(webSocket, filter, CancellationToken.None);
                }
            }
            else
            {
                ControllerContext.HttpContext.Response.StatusCode = 400;
            }
        }
    }
}
