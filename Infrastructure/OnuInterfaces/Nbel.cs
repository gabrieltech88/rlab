using System.Text;
using PrimS.Telnet;
using RLab.Abstractions;

namespace RLab.Infrastructure.OnuInterfaces;

public class Nbel : INbel
{
    public string Model => throw new NotImplementedException();
    public bool hasDigitalCertificate { get; } = false;

    public Task<bool> ChangeWlanAndPPPoE(string ip, int position)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> ConfigureAsync(string model, string ip)
    {
       using var cancellationTokenSource = new CancellationTokenSource();

        using var client = new Client(
            ip,
            23,
            cancellationTokenSource.Token
        );

        if (!client.IsConnected)
            throw new Exception("Não foi possível conectar via Telnet.");

        Console.WriteLine("Conectado via Telnet.");

        // Limpa/consome o prompt inicial da ONU antes de começar.
        var initialResponse = await ReadUntilPrompt(client);

        Console.WriteLine("Prompt inicial recebido:");
        Console.WriteLine(initialResponse);

        var commands = await File.ReadAllLinesAsync($"C:/Users/Rapid/Documents/Dev/RLab/Lib/{model}/{model}.txt");

        Console.WriteLine($"Comandos carregados: {commands.Length}");
        Console.WriteLine("Iniciando envio de comandos...");

        for (var i = 0; i < commands.Length; i++)
        {
            var command = commands[i].Trim();

            if (string.IsNullOrWhiteSpace(command))
                continue;

            Console.WriteLine();
            Console.WriteLine($"[{i + 1}/{commands.Length}] > {command}");

            await client.WriteLineAsync(command);

            // Reboot é especial porque a ONU pode derrubar
            // a conexão antes de retornar outro /sbin #
            if (command.Equals(
                    "reboot",
                    StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Comando de reboot enviado.");
                await Task.Delay(3000);
                break;
            }

            try
            {
                var response = await ReadUntilPrompt(client);

                Console.WriteLine("Resposta:");
                Console.WriteLine(response);

                Console.WriteLine("Comando finalizado.");
                
            }
            catch (TimeoutException ex)
            {
                Console.WriteLine();
                Console.WriteLine("ERRO:");
                Console.WriteLine(ex.Message);

                Console.WriteLine(
                    $"Execução interrompida no comando {i + 1}: {command}"
                );

                break;
            }


        }

        Console.WriteLine();
        Console.WriteLine("Processamento encerrado.");
        return true;
    }

    private static async Task<string> ReadUntilPrompt(Client client)
    {
        var response = new StringBuilder();

        var timeoutAt = DateTime.UtcNow.AddSeconds(15);

        while (DateTime.UtcNow < timeoutAt)
        {
            var chunk = await client.ReadAsync(
                TimeSpan.FromMilliseconds(500)
            );

            if (string.IsNullOrEmpty(chunk))
                continue;

            response.Append(chunk);

            if (response
                .ToString()
                .Contains("/sbin #", StringComparison.OrdinalIgnoreCase))
            {
                return response.ToString();
            }
        }

        throw new TimeoutException(
            "Timeout: a ONU não retornou o prompt '/sbin #' dentro de 15 segundos.\n" +
            $"Resposta recebida até o momento:\n{response}"
        );
    }

    public Task<bool> UploadDigitalCertificate(string ip)
    {
        throw new NotImplementedException();
    }
}