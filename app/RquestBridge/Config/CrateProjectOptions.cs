namespace RquestBridge.Config;

public class CrateProjectOptions
{


    public string Id { get; set; } = string.Empty;

    public string Type { get; set; } = "Project";

    public List<Identifier> Identifiers { get; set; } = new List<Identifier>();

    public FundingSource Funding { get; set; } = new FundingSource();

    public List<Organization> Member { get; set; } = new List<Organization>();
}

public class Identifier
{
    public string Id { get; set; } = string.Empty;

    public string Type { get; set; } = "PropertyValue";

    public string Name { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;
}


