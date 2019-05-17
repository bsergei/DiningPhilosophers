using System;
using System.Threading;
using System.Threading.Tasks;
using DiningPhilosophers.Sim.Model;
using DiningPhilosophers.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace DiningPhilosophers.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TableController : ControllerBase
    {
        private readonly IRunnerService runnerService_;
        

        public TableController(IRunnerService runnerService)
        {
            runnerService_ = runnerService;
        }

        [HttpGet("start/{tableType}")]
        public ActionResult<StateDto> StartTable(
            TableType tableType, 
            [FromQuery] int? simulationTime = null,
            [FromQuery] int philosophersCount = 10)
        {
            var timeout = simulationTime == null
                ? Timeout.InfiniteTimeSpan
                : TimeSpan.FromSeconds(simulationTime.Value);
            
            var simulationTimeCts = new CancellationTokenSource(timeout);

            var tableId = runnerService_.Start(
                tableType,
                philosophersCount, 
                simulationTimeCts.Token,
                () =>
                {
                    simulationTimeCts.Dispose();
                    return Task.CompletedTask;
                });

            return Ok(tableId);
        }

        [HttpGet("stop/{tableId}")]
        public async Task<ActionResult> StopTable(Guid tableId)
        {
            await runnerService_.Stop(tableId);
            return Ok();
        }
    }
}
