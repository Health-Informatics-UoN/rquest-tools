using System.IO.Compression;
using System.Text.Json;
using RquestBridge.Config;
using RquestBridge.Constants;
using RquestBridge.Dto.RQuestResults;
using RquestBridge.Models.WebHooks;

namespace RquestBridge.Services;

public class ResultsHandlingService(MinioService minioService, BridgeOptions bridgeOptions,
  RQuestTaskApiClient rQuestTaskApiClient, ILogger logger)
{
  /// <summary>
  /// Download the final results of a query from S3 and POST them back to RQuest.
  /// </summary>
  /// <param name="payload">
  /// An object describing the final outcome of a request coming from a configured web hook.
  /// </param>
  public async Task HandleResults(FinalOutcomeWebHookModel payload)
  {
    var pathToResults = Path.Combine(bridgeOptions.WorkingDirectoryBase, payload.File);
    var resultsDirectory =
      Path.Combine(bridgeOptions.WorkingDirectoryBase, Path.GetFileNameWithoutExtension(pathToResults));
    var resultsFile = Path.Combine(resultsDirectory, "data", RquestQuery.ResultFileName);

    // Get file from S3
    await minioService.GetFromStore(payload.File, pathToResults);

    // Unzip results
    ZipFile.ExtractToDirectory(pathToResults, resultsDirectory);

    // Read results object
    var resultsJson = await File.ReadAllTextAsync(resultsFile);

    var results = JsonSerializer.Deserialize<RquestQueryResult>(resultsJson);
    if (results is null)
    {
      logger.LogError("Could not deserialise results from {File}", resultsFile);
      return;
    }

    // POST to RQuest
    await rQuestTaskApiClient.ResultsEndpointPost(payload.SubId, results);

    // Delete unpacked results
    Directory.Delete(resultsDirectory, recursive: true);
  }
}
