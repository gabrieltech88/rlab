using Microsoft.Playwright;
using RLab.Abstractions;

namespace RLab.Infrastructure.OnuInterfaces;

public class HuaweiRed : IHuaweiRed
{
    public string Model => throw new NotImplementedException();
    public bool hasDigitalCertificate { get; } = true;

    public Task<bool> ChangeWlanAndPPPoE(string ip, int position)
    {
        throw new NotImplementedException();
    }
    public async Task<bool> ConfigureAsync(string model, string ip)
    {
        using var playwright = await Playwright.CreateAsync();

        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false
        });

        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true
        });

        var page = await context.NewPageAsync();

        await page.GotoAsync($"http://{ip}");

        await page.Locator("#txt_Username").FillAsync("Epadmin");
        await page.Locator("#txt_Password").FillAsync("adminEp");
        await page.Locator("#button").ClickAsync();

        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        await page.Locator("li[name='mainli_reset'][value='10']").ClickAsync();
        await page.Locator("li[name='subli_cfgfile'][value='1']").ClickAsync();

        var frame = page.FrameLocator("#frameContent");

        await frame.Locator("#t_file").SetInputFilesAsync($"C:/Users/Rapid/Documents/Dev/RLab/Lib/{model}/{model}.xml");

        await Task.Delay(1000);

        page.Dialog += async (_, dialog) =>
        {
            await dialog.AcceptAsync();
        };

        await frame.Locator("#btnSubmit").ClickAsync();

        await Task.Delay(5000);
        await browser.CloseAsync();

        return true;
    }

    public Task<bool> UploadDigitalCertificate(string ip)
    {
        throw new NotImplementedException();
    }
}