using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RquestBridge.Models.WebHooks;

namespace RquestBridge.Controllers;

[ApiController]
[AllowAnonymous]
[Produces("application/json")]
[Route("api/{controller}")]
public class JobsController : ControllerBase
{
  [HttpPost("complete")]
  public async Task<IActionResult> JobComplete(FinalOutcomeWebHookModel payload)
  {
    // Todo: trigger services to handle the completed job
    return NoContent();
  }
}
