namespace Blaganet.Identity.Configuration;

public class FunctionOptions
{
    public required string Name { get; set; }
    public bool Anonymous { get; set; } = false;
    public string[] Requirements { get; set; } = [];
}