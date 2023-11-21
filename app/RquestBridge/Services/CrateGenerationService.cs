using System.Text.Json;
using FiveSafes.Net;
using RquestBridge.Dto;
using RquestBridge.Config;
namespace RquestBridge.Services;

public class CrateGenerationService
{
  public CrateGenerationService(){}

  public async Task BuildCrate<T>(T job)
  {
    // TODO: build the initial archive

    var payload = JsonSerializer.Serialize<T>(job);
    var destination = Path.Combine("someDir", RquestQueryOptions.FileName);
    await SaveJobPayload(payload, destination);
  }

  /// <summary>
  /// Save a job payload to a file on disk.
  /// </summary>
  /// <param name="payload">The string to write to the file.</param>
  /// <param name="destination">The path to the file to be created.</param>
  /// <returns></returns>
  private async Task SaveJobPayload(string payload, string destination)
  {
    var destinationInfo = new FileInfo(destination);

    using var fileStream = destinationInfo.Create();
    using var writer = new StreamWriter(fileStream);
    await writer.WriteAsync(payload);
  }
}
