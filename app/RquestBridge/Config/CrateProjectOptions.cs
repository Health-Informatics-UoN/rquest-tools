using ROCrates.Models;

namespace RquestBridge.Config;

public class CrateProjectOptions
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = "Project";
    public string Name { get; set; } = string.Empty;
    public List<Identifier> Identifiers { get; set; } = new();
    public FundingSource Funding { get; set; } = new();
    public List<Part> Member { get; set; } = new();
}

public class Identifier
{
    public string Id { get; set; } = string.Empty;

    public string Type { get; set; } = "PropertyValue";

    public string Name { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;
}

public class FundingSource
{
    public string Id { get; set; } = string.Empty;

    public string Type { get; set; } = "Grant";

    public string Name { get; set; } = string.Empty;
}


