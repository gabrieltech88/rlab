namespace RLab.Infrastructure.Orchestrator;
public class OnuPipelineItem
{
    public int Position { get; set; }
    public string Ip { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;

    public bool ConfigurationUploaded { get; set; }
    public bool Configured { get; set; }
    public bool Provisioned { get; set; }

    public string? ConfigurationError { get; set; }
    public string? ProvisioningError { get; set; }
}