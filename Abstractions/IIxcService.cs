namespace RLab.Abstractions;
public interface IIxcService
{
    Task<bool> ProvisionOnuAsync();
}