using System.Text.Json;
using FiveSafes.Net;
using Microsoft.Extensions.Logging;
using ROCrates;
using ROCrates.Exceptions;
using RquestBridge.Config;
namespace RquestBridge.Services;

public class CrateGenerationService
{
  private ILogger<CrateGenerationService> _logger;
  public CrateGenerationService(ILogger<CrateGenerationService> logger) 
  { 
    _logger = logger;
  }

  public async Task BuildCrate<T>(T job)
  {

    var archive = await BuildBagIt(BridgeOptions.WorkingDirectory);
    // checksum files (blank)
    // data/
    //    metadata.json
    //    preview.html

    var payload = JsonSerializer.Serialize<T>(job);
    var payloadDestination = Path.Combine(archive.PayloadDirectoryPath, RquestQueryOptions.FileName);
    await SaveJobPayload(payload, payloadDestination);
    
    // Generate ROCrate metadata
    var roCrate = InitialiseCrate(archive.PayloadDirectoryPath);

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

  private ROCrate InitialiseCrate(string cratePath)
  {
    var crate = new ROCrate();
    try
    {
      crate.Initialise(cratePath);
    }
    catch (CrateReadException e)
    {
      _logger.LogError(exception: e, "RO-Crate cannot be read, or is invalid");
      throw;
    }
    catch (MetadataException e)
    {
      _logger.LogError(exception: e, "RO-Crate Metadata cannot be read, or is invalid");
      throw;
    }

    return crate;
  }
}
