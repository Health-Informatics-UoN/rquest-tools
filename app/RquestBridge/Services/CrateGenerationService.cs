using System.Text.Json;
using FiveSafes.Net;
using RquestBridge.Config;
namespace RquestBridge.Services;

public class CrateGenerationService
{
  public CrateGenerationService() { }

  public async Task BuildCrate<T>(T job)
  {

    var archive = await BuildBagIt(BridgeOptions.WorkingDirectory);

    var payload = JsonSerializer.Serialize<T>(job);
    var payloadDestination = Path.Combine(archive.PayloadDirectoryPath, RquestQueryOptions.FileName);
    await SaveJobPayload(payload, payloadDestination);
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

  /// <summary>
  /// 
  /// </summary>
  /// <param name="destination"></param>
  /// <returns></returns>
  private async Task<BagItArchive> BuildBagIt(string destination)
  {
    var builder = new FiveSafesBagItBuilder(destination);
    var packer = new Packer(builder);
    await packer.BuildBlankArchive();
    return builder.GetArchive();
  }
}
