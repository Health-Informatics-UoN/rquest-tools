using ROCrates.Models;

namespace RquestBridge.Config;

public class CrateAgentOptions
{

    public string Id { get; set; } = string.Empty;

    public string Type { get; set; } = "Person";

    public string Name { get; set; } = string.Empty;

    public Part Affiliation { get; set; } = new();

    public List<Part> MemberOf { get; set; } = new();

}



