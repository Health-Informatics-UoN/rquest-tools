using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Minio.Exceptions;
using RquestBridge.Models.WebHooks;
using RquestBridge.Services;

namespace RquestBridge.Controllers;

[ApiController]
[AllowAnonymous]
[Produces("application/json")]
[Route("api/{controller}")]
public class JobsController(ResultsHandlingService resultsHandlingService) : ControllerBase
{
  [HttpPost("complete")]
  public async Task<IActionResult> JobComplete(FinalOutcomeWebHookModel payload)
  {
    try
    {
      await resultsHandlingService.HandleResults(payload);
      return NoContent();
    }
    catch (Exception e) when (e is MinioException)
    {
      /*
       * `payload` contains invalid credentials for the configured MinIO instance
       * or points to a non-existent bucket or object.
       */
      return BadRequest(e);
    }
  }
}
