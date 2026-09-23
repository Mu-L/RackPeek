using Microsoft.Playwright;
using Tests.E2e.Infra;
using Tests.E2e.PageObjectModels;
using Xunit.Abstractions;

namespace Tests.E2e;

public class OtherTests(
    PlaywrightFixture fixture,
    ITestOutputHelper output) : E2ETestBase(fixture, output) {
    private readonly PlaywrightFixture _fixture = fixture;
    private readonly ITestOutputHelper _output = output;

    [Fact]
    public async Task User_Can_Add_And_Delete_Other() {
        (IBrowserContext context, IPage page) = await CreatePageAsync();
        var resourceName = $"e2e-other-{Guid.NewGuid():N}"[..16];

        try {
            // Go home
            await page.GotoAsync(_fixture.BaseUrl);

            _output.WriteLine($"URL after Goto: {page.Url}");

            var layout = new MainLayoutPom(page);
            await layout.AssertLoadedAsync();
            await layout.GotoHardwareAsync();

            var hardwarePage = new HardwareTreePom(page);
            await hardwarePage.AssertLoadedAsync();
            await hardwarePage.GotoOtherListAsync();

            var listPage = new OtherListPom(page);
            await listPage.AssertLoadedAsync();
            await listPage.AddOtherAsync(resourceName);
            await listPage.AssertOtherExists(resourceName);
            await listPage.DeleteOtherAsync(resourceName);
            await listPage.AssertOtherDoesNotExist(resourceName);

            await context.CloseAsync();
        }
        catch (Exception) {
            _output.WriteLine("TEST FAILED — Capturing diagnostics");

            _output.WriteLine($"Current URL: {page.Url}");

            var html = await page.ContentAsync();
            _output.WriteLine("==== DOM SNAPSHOT START ====");
            _output.WriteLine(html);
            _output.WriteLine("==== DOM SNAPSHOT END ====");

            throw;
        }
        finally {
            await context.CloseAsync();
        }
    }
}
