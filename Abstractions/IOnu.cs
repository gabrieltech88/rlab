namespace RLab.Abstractions;

public interface IOnu
{
    string Model { get; }
    bool hasDigitalCertificate { get; }

    public Task<bool> ConfigureAsync(string model, string ip);
    public Task<bool> ChangeWlanAndPPPoE(string ip, int position);
    public Task<bool> UploadDigitalCertificate(string ip);

}