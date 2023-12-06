using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RquestBridge.Models.WebHooks;
using RquestBridge.Services;

namespace RquestBridge.Controllers;

[ApiController]
[AllowAnonymous]
[Produces("application/json")]
[Route("api/{controller}")]
public class JobsController(ResultsHandlingService resultsHandlingService, ILogger logger) : ControllerBase
{
  [HttpPost("complete")]
  public async Task<IActionResult> JobComplete(FinalOutcomeWebHookModel payload)
  {
    await resultsHandlingService.HandleResults(payload);
    return NoContent();
  }
}
