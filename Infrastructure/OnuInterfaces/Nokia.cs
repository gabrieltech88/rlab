using Microsoft.Playwright;
using RLab.Abstractions;

namespace RLab.Infrastructure.OnuInterfaces;

public class Nokia : INokia
{
    public string Model => throw new NotImplementedException();

    public bool hasDigitalCertificate { get; } = false;

    public Task<bool> ChangeWlanAndPPPoE(string ip, int position)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> ConfigureAsync(string model, string ip)
    {
        using var playwright = await Playwright.CreateAsync();

        await using var browser = await playwright.Chromium.LaunchAsync(
            new BrowserTypeLaunchOptions
            {
                Headless = false
            });

        var context = await browser.NewContextAsync(
            new BrowserNewContextOptions
            {
                IgnoreHTTPSErrors = true
            });

        var page = await context.NewPageAsync();

        await page.GotoAsync($"http://{ip}");

        await page.Locator("#username").FillAsync("AdminGPON");
        await page.Locator("#password").FillAsync("ALC#FGU");
        await page.Locator("#loginBT").ClickAsync();

        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        await page
            .Locator("li.x_main_menu a")
            .Filter(new() { HasText = "Maintenance" })
            .ClickAsync();

        await page
            .Locator("#div_menu_left a[href='usb.cgi?backup']")
            .ClickAsync();

        var frame = page.Frame("mainFrame");

        if (frame is null)
            throw new Exception("mainFrame não encontrado.");

        await frame.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        var fileInput = frame.Locator("input[type='file']");

        await fileInput.SetInputFilesAsync($"C:/Users/Rapid/Documents/Dev/RLab/Lib/{model}/{model}.cfg");

        page.Dialog += async (_, dialog) =>
        {
            Console.WriteLine($"Alert recebido: {dialog.Message}");
            await dialog.AcceptAsync();
        };

        // Clica no botão de importação
        await frame.Locator("#imp").ClickAsync();
        Task.Delay(1000);

        Console.WriteLine("Importação acionada.");

        await Task.Delay(5000); throw new NotImplementedException();
    }

    public Task<bool> UploadDigitalCertificate(string ip)
    {
        throw new NotImplementedException();
    }

}