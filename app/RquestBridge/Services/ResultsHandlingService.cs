using System.IO.Compression;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RquestBridge.Config;
using RquestBridge.Constants;
using RquestBridge.Dto.RQuestResults;
using RquestBridge.Models.WebHooks;

namespace RquestBridge.Services;

public class ResultsHandlingService(MinioService minioService, IOptions<BridgeOptions> bridgeOptions,
  RQuestTaskApiClient rQuestTaskApiClient, ILogger<ResultsHandlingService> logger)
{
  private readonly BridgeOptions _bridgeOptions = bridgeOptions.Value;

  /// <summary>
  /// Download the final results of a query from S3 and POST them back to RQuest.
  /// </summary>
  /// <param name="payload">
  /// An object describing the final outcome of a request coming from a configured web hook.
  /// </param>
  public async Task HandleResults(FinalOutcomeWebHookModel payload)
  {
    var pathToResults = Path.Combine(_bridgeOptions.WorkingDirectoryBase, $"{payload.SubId}-{payload.File}");
    var resultsDirectory =
      Path.Combine(_bridgeOptions.WorkingDirectoryBase, Path.GetFileNameWithoutExtension(pathToResults));

    // Get file from S3
    await minioService.GetFromStore(payload.File, pathToResults);

    // Unzip results
    ZipFile.ExtractToDirectory(pathToResults, resultsDirectory);

    // Get results file
    var directoryInfo = new DirectoryInfo(Path.Combine(resultsDirectory, "data"));

    var resultsFile = directoryInfo.EnumerateFiles($"outputs/outputs/*", SearchOption.AllDirectories).First();

    // Read results object
    var resultsJson = await File.ReadAllTextAsync(resultsFile.FullName);

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
