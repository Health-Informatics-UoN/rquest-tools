using ROCrates.Models;

namespace RquestBridge.Config;

public class CrateAgentOptions
{

    public string Id { get; set; } = string.Empty;

    public string Type { get; set; } = "Person";

    public string Name { get; set; } = string.Empty;

    public CrateOrganizationOptions Affiliation { get; set; } = new CrateOrganizationOptions();

    public List<CrateProjectOptions> MemberOf { get; set; } = new List<CrateProjectOptions>();

}



