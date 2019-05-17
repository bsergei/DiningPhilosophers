using System.Threading.Tasks;
using DiningPhilosophers.Sim.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace DiningPhilosophers.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TableStateController : ControllerBase
    {
        private readonly IStateRepository stateRepository_;

        public TableStateController(IStateRepository stateRepository)
        {
            stateRepository_ = stateRepository;
        }

        [HttpGet]
        public async Task<ActionResult> Status()
        {
            var result = await stateRepository_.Get();
            return Ok(result);
        }
    }
}
