using System.ComponentModel.DataAnnotations;
using RquestBridge.Config;

namespace RquestBridge.Models;

public class HutchJob
{
  /// <summary>
  /// This is a Submission ID matching the RQuest Job Uuid
  /// </summary>
  [Required]
  public string SubId { get; set; } = string.Empty;

  /// <summary>
  /// Optional Project Database Connection details.
  /// This allows the requested workflow to be granted access to the
  /// Hutch data source it should run against, if any.
  /// </summary>
  public HutchDatabaseConnectionDetails? DataAccess { get; set; }

  /// <summary>
  /// Details for where the crate can be found in a Cloud Storage Provider.
  /// </summary>
  public FileStorageDetails? CrateSource { get; set; }
}
