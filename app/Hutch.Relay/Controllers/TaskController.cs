namespace Hutch.Relay.Controllers;

using Rackit.TaskApi.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

[ApiController]
[Route("/[controller]")]
public class TaskController : ControllerBase
{
  [HttpGet("nextjob/{collectionId}")]
  [SwaggerOperation("Fetch next job from queue.")]
  [SwaggerResponse(200)]
  [SwaggerResponse(204)]
  [SwaggerResponse(401)]
  [SwaggerResponse(403)]
  [SwaggerResponse(404)]
  [SwaggerResponse(500)]
  public async Task<IActionResult> Next(string collectionId)
  {
    return await DummyNext(collectionId);
  }

  [HttpPost("result/{uuid}/{collectionId}")]
  [SwaggerOperation("Upload job result by UUID and CollectionId.")]
  [SwaggerResponse(200)]
  [SwaggerResponse(401)]
  [SwaggerResponse(403)]
  [SwaggerResponse(404)]
  [SwaggerResponse(409)]
  public async Task<IActionResult> Result(string uuid, string collectionId, [FromBody] JobResult result)
  {
    throw new NotImplementedException();
  }

  # region Dummy Endpoints

  private static readonly Random _random = new();

  private static AvailabilityJob _dummyAvailability = new()
  {
    Uuid = Guid.NewGuid().ToString(),
    Cohort = new()
    {
      Combinator = "OR",
      Groups =
      [
        new Group()
        {
          Combinator = "AND",
          Rules =
          [
            new Rule()
            {
              Type = "TEXT",
              VariableName = "OMOP",
              Operand = "=",
              Value = "260139",
            }
          ]
        }
      ]
    },
    Owner = "user1",
    CharSalt = "52ee5332-d209-4cf6-86d2-7f7569292c23",
    ProtocolVersion = "v2"
  };

  private const CollectionAnalysisJob _dummyGeneric =
    // """
    // {
    //   "code": "GENERIC",
    //   "analysis": "DISTRIBUTION",
    //   "uuid": "b152db31-e3ef-4ec7-9bed-c938cc2c0ac2",
    //   "collection": "RQ-CC-d8e8c4aa-a8af-4229-b479-618788c44122",
    //   "owner": "user1"
    // }
    // """;

  private const CollectionAnalysisJob _dummyDemographics =
    // """
    // {
    //   "code": "DEMOGRAPHICS",
    //   "analysis": "DISTRIBUTION",
    //   "uuid": "d16d3db6-0b07-4cea-be66-f1b54763e71c",
    //   "collection": "RQ-CC-d8e8c4aa-a8af-4229-b479-618788c44122",
    //   "owner": "user1"
    // }
    // """;

  /// <summary>
  /// A dummy endpoint used for rapid testing.
  /// Currently supports Availability and Generic/Demographics Distribution queries only.
  ///
  /// Randomly returns nothing or a valid test payload for the nextjob, as if interacting with a queue.
  /// Unsupported query types always return 204.
  /// </summary>
  /// <returns></returns>
  private Task<IActionResult> DummyNext(string collectionId)
  {
    // flip a coin and return nothing if false
    if (!Flip()) return Task.FromResult<IActionResult>(NoContent());

    // Otherwise, return based on the requested type
    var queryType = collectionId.Split(".").Last();

    var payload = queryType switch
    {
      "a" => _dummyAvailability,
      "b" => Flip() ? _dummyGeneric : _dummyDemographics, // random chance which type of distribution query
      _ => queryType == collectionId ? _dummyAvailability : null
    };

    return payload is null
      ? Task.FromResult<IActionResult>(NoContent())
      : Task.FromResult<IActionResult>(Ok(payload));

    bool Flip() => _random.Next(0, 1) == 1;
  }

  #endregion
}
