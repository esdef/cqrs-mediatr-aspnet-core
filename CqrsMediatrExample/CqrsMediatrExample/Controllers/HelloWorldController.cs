using CqrsMediatrExample.Commands;
using CqrsMediatrExample.Notifications;
using CqrsMediatrExample.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CqrsMediatrExample.Controllers
{
    [Route("api/hello-world")]
    [ApiController]
    public class HelloWorldController : ControllerBase
    {
        private readonly IMediator _mediator;

        public HelloWorldController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        public async Task<ActionResult> GetHelloWorld()
        {
            var helloWorld = await _mediator.Send(new GetHelloWorldQuery());

            return Ok(helloWorld);
        }        
    }
}
