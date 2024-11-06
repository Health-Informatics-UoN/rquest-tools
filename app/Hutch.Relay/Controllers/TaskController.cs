namespace Hutch.Relay.Controllers;

using Rackit.TaskApi.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;


[ApiController]
[Route("/[controller]")]
public class TaskController : ControllerBase
{
  [HttpGet("nextjob/{collectionId:int}")]
  [SwaggerOperation("Fetch next job from queue.")]
  [SwaggerResponse(200)]
  [SwaggerResponse(401)]
  [SwaggerResponse(403)]
  [SwaggerResponse(404)]
  [SwaggerResponse(500)]
  public IActionResult Next(int collectionId)
  {
    throw new NotImplementedException();
  }

  [HttpPost("result/{uuid}/{collectionId:int}")]
  [SwaggerOperation("Upload job result by UUID and CollectionId.")]
  [SwaggerResponse(200)]
  [SwaggerResponse(401)]
  [SwaggerResponse(403)]
  [SwaggerResponse(404)]
  [SwaggerResponse(409)]
  public IActionResult Result(string uuid, int collectionId, [FromBody] JobResult result)
  {
    throw new NotImplementedException();
  }
}
