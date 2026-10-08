using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using RLab.Abstractions;
using RLab.Enums;

namespace RLab.Infrastructure.Orchestrator;

public class Orchestrator
{
    private readonly IOnuInterfaceFactory _factory;
    private readonly IHubContext<OrchestratorHub> _hubContext;
    private readonly IIxcService _ixcService;
    private readonly ILogger<Orchestrator> _logger;

    public Orchestrator(IHubContext<OrchestratorHub> hubContext, IOnuInterfaceFactory factory, IIxcService ixcService, ILogger<Orchestrator> logger)
    {
        _hubContext = hubContext;
        _factory = factory;
        _ixcService = ixcService;
        _logger = logger;
    }

    public async Task RunAsync(string model, int numberOfOnus)
    {
        if (string.IsNullOrWhiteSpace(model))
            throw new ArgumentException("O modelo da ONU deve ser informado.", nameof(model));

        if (numberOfOnus is < 1 or > 8)
            throw new ArgumentOutOfRangeException(nameof(numberOfOnus), "A quantidade de ONUs deve estar entre 1 e 8.");

        var pipeline = BuildPipeline(model, numberOfOnus);

        _logger.LogInformation("Iniciando fluxo automatizado. Modelo: {Model}. Quantidade de ONUs: {NumberOfOnus}", model, numberOfOnus);

        await SendLogAsync(State.Started, "Início do fluxo automatizado.");

        try
        {
            IOnu onuInterface = _factory.Create(model);

            _logger.LogInformation("Interface da ONU criada com sucesso para o modelo {Model}", model);

            foreach (var item in pipeline)
            {
                try
                {
                    _logger.LogInformation("Iniciando upload da configuração da ONU na ether{Ether}. IP: {Ip}. Modelo: {Model}", item.Position + 1, item.Ip, item.Model);

                    await SendLogAsync(State.Configuring, $"Subindo arquivo de configuração na ONU na ether{item.Position + 1}...");

                    var isConfigured = await onuInterface.ConfigureAsync(item.Model, item.Ip);

                    if (!isConfigured)
                        throw new Exception($"Não foi possível subir o arquivo de configuração na ONU na ether{item.Position + 1}");

                    item.ConfigurationUploaded = true;

                    _logger.LogInformation("Arquivo de configuração enviado com sucesso para a ONU na ether{Ether}. IP: {Ip}", item.Position + 1, item.Ip);

                    await SendLogAsync(State.Configured, $"Arquivo de configuração upado na ONU na ether{item.Position + 1}");

                    /*
                    if (onuInterface.hasDigitalCertificate)
                    {
                        try
                        {
                            _logger.LogInformation("Iniciando upload do certificado digital da ONU na ether{Ether}. IP: {Ip}", item.Position + 1, item.Ip);

                            await SendLogAsync(State.Configuring, $"Subindo certificado digital na ONU na ether{item.Position + 1}...");

                            await onuInterface.UploadDigitalCertificate(item.Ip);

                            _logger.LogInformation("Certificado digital enviado com sucesso para a ONU na ether{Ether}. IP: {Ip}", item.Position + 1, item.Ip);

                            await SendLogAsync(State.Configured, $"Certificado digital upado na ONU na ether{item.Position + 1}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Erro ao enviar certificado digital para a ONU na ether{Ether}. IP: {Ip}", item.Position + 1, item.Ip);

                            throw;
                        }
                    }
                    */
                }
                catch (Exception ex)
                {
                    item.ConfigurationUploaded = false;
                    item.Configured = false;
                    item.ConfigurationError = ex.Message;

                    _logger.LogError(ex, "Erro ao configurar a ONU na ether{Ether}. IP: {Ip}. Modelo: {Model}", item.Position + 1, item.Ip, item.Model);

                    await SendLogAsync(State.Error, $"Não foi possível configurar a ONU na ether{item.Position + 1}: {ex.Message}");
                }
            }

            /*
            foreach (var item in pipeline)
            {
                if (!item.ConfigurationUploaded)
                continue;

                try
                {
                    _logger.LogInformation("Iniciando alteração de PPPoE e WLAN da ONU na ether{Ether}. IP: {Ip}", item.Position + 1, item.Ip);

                    await SendLogAsync(State.Configuring, $"Trocando o PPPoE e WLAN da ONU na ether{item.Position + 1}...");

                    bool isConfigured = await onuInterface.ChangeWlanAndPPPoE(item.Ip, item.Position);

                    if (!isConfigured)
                        throw new Exception($"Não foi possível trocar o PPPoE e WLAN da ONU na ether{item.Position + 1}");

                    item.Configured = true;

                    _logger.LogInformation("PPPoE e WLAN alterados com sucesso na ONU na ether{Ether}. IP: {Ip}", item.Position + 1, item.Ip);

                    await SendLogAsync(State.Configured, $"ONU na ether{item.Position + 1} configurada com sucesso.");
                }
                catch (Exception ex)
                {
                    item.Configured = false;
                    item.ConfigurationError = ex.Message;

                    _logger.LogError(ex, "Erro ao alterar PPPoE e WLAN da ONU na ether{Ether}. IP: {Ip}", item.Position + 1, item.Ip);

                    await SendLogAsync(State.Error, $"Não foi possível trocar o PPPoE e WLAN da ONU na ether{item.Position + 1}: {ex.Message}");
                }
            }

            foreach (var item in pipeline)
            {
                if (!item.Configured)
                    continue;

                try
                {
                    _logger.LogInformation("Iniciando provisionamento da ONU na ether{Ether}. IP: {Ip}", item.Position + 1, item.Ip);

                    await SendLogAsync(State.Configuring, $"Provisionando ONU na ether{item.Position + 1}...");

                    bool isProvisioned = await _ixcService.ProvisionOnuAsync();

                    if (!isProvisioned)
                        throw new Exception($"Não foi possível provisionar a ONU na ether{item.Position + 1}");

                    item.Provisioned = true;

                    _logger.LogInformation("ONU provisionada com sucesso na ether{Ether}. IP: {Ip}", item.Position + 1, item.Ip);

                    await SendLogAsync(State.Configured, $"ONU na ether{item.Position + 1} provisionada com sucesso.");
                }
                catch (Exception ex)
                {
                    item.Provisioned = false;
                    item.ProvisioningError = ex.Message;

                    _logger.LogError(ex, "Erro ao provisionar a ONU na ether{Ether}. IP: {Ip}", item.Position + 1, item.Ip);

                    await SendLogAsync(State.Error, $"Não foi possível provisionar a ONU na ether{item.Position + 1}: {ex.Message}");
                }
            }
            */
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro global durante o fluxo automatizado. Modelo: {Model}", model);

            await SendLogAsync(State.Error, $"Erro no fluxo automatizado: {ex.Message}");
        }
        finally
        {
            _logger.LogInformation("Fluxo automatizado finalizado. Modelo: {Model}. Quantidade de ONUs: {NumberOfOnus}", model, numberOfOnus);

            await SendLogAsync(State.Finished, "Fim do fluxo automatizado.");
        }
    }

    private List<OnuPipelineItem> BuildPipeline(string model, int numberOfOnus)
    {
        var pipeline = new List<OnuPipelineItem>();

        for (var i = 0; i < numberOfOnus; i++)
        {
            // Ip = $"10.0.0.{i + 2}",

            pipeline.Add(new OnuPipelineItem
            {
                Position = i + 1,
                Ip = $"10.0.0.1{i + 1}",
                Model = model
            });
        }

        return pipeline;
    }

    private async Task SendLogAsync(State state, string message)
    {
        try
        {
            await _hubContext.Clients.All.SendAsync("ReceiveLog", state, message);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao enviar mensagem via SignalR. State: {State}. Mensagem: {Message}", state, message);
        }
    }
}