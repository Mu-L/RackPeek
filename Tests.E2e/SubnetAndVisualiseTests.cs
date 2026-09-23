using Microsoft.Playwright;
using Tests.E2e.Infra;
using Tests.E2e.PageObjectModels;
using Xunit.Abstractions;

namespace Tests.E2e;

/// <summary>
///     Coverage for the two read-only overview pages, /subnets and /visualise.
///     Both derive entirely from the resources in the config, so the tests
///     create what they assert on. Kept in one class so they share a container.
/// </summary>
public class SubnetAndVisualiseTests(
    PlaywrightFixture fixture,
    ITestOutputHelper output) : E2ETestBase(fixture, output) {
    private readonly PlaywrightFixture _fixture = fixture;
    private readonly ITestOutputHelper _output = output;

    // =============================================================
    // Subnet browser
    // =============================================================

    [Fact]
    public async Task Subnet_Browser_Groups_A_System_Under_Its_Slash_24() {
        (IBrowserContext context, IPage page) = await CreatePageAsync();
        var name = $"e2e-sn-{Guid.NewGuid():N}"[..14];
        const string ip = "10.99.4.17";
        const string subnet = "10.99.4.x";

        try {
            await CreateSystemWithIpAsync(page, name, ip);

            await page.GotoAsync($"{_fixture.BaseUrl}/subnets");

            var subnets = new SubnetBrowserPom(page);
            await subnets.AssertLoadedAsync();

            await subnets.AssertGroupVisibleAsync(subnet);
            await subnets.AssertGroupContainsAsync(subnet, ip);
            await subnets.AssertGroupContainsAsync(subnet, name);
        }
        catch (Exception) {
            await DumpAsync(page);
            throw;
        }
        finally {
            await context.CloseAsync();
        }
    }

    [Fact]
    public async Task Subnet_Browser_Filter_Narrows_And_Can_Match_Nothing() {
        (IBrowserContext context, IPage page) = await CreatePageAsync();
        var name = $"e2e-sf-{Guid.NewGuid():N}"[..14];
        const string ip = "10.88.3.9";
        const string subnet = "10.88.3.x";

        try {
            await CreateSystemWithIpAsync(page, name, ip);

            await page.GotoAsync($"{_fixture.BaseUrl}/subnets");

            var subnets = new SubnetBrowserPom(page);
            await subnets.AssertLoadedAsync();
            await subnets.AssertGroupVisibleAsync(subnet);

            // A matching prefix keeps the group.
            await subnets.FilterAsync("10.88.3");
            await subnets.AssertGroupVisibleAsync(subnet);

            // A subnet nothing lives in falls through to the empty state.
            await subnets.FilterAsync("203.0.113");
            await subnets.AssertEmptyAsync();
        }
        catch (Exception) {
            await DumpAsync(page);
            throw;
        }
        finally {
            await context.CloseAsync();
        }
    }

    // =============================================================
    // Visualise
    // =============================================================

    [Fact]
    public async Task User_Can_Switch_Between_Visualise_Views() {
        (IBrowserContext context, IPage page) = await CreatePageAsync();

        try {
            await page.GotoAsync($"{_fixture.BaseUrl}/visualise");

            var visualise = new VisualisePom(page);
            await visualise.AssertLoadedAsync();

            await visualise.SelectLogicalAsync();
            await visualise.AssertLoadedAsync();

            await visualise.SelectTopologyAsync();
            await visualise.AssertLoadedAsync();
        }
        catch (Exception) {
            await DumpAsync(page);
            throw;
        }
        finally {
            await context.CloseAsync();
        }
    }

    [Fact]
    public async Task Visualise_Deep_Links_Straight_To_A_View() {
        (IBrowserContext context, IPage page) = await CreatePageAsync();

        try {
            await page.GotoAsync($"{_fixture.BaseUrl}/visualise/logical");

            var visualise = new VisualisePom(page);
            await visualise.AssertLoadedAsync();
        }
        catch (Exception) {
            await DumpAsync(page);
            throw;
        }
        finally {
            await context.CloseAsync();
        }
    }

    // =============================================================
    // Helpers
    // =============================================================

    private async Task CreateSystemWithIpAsync(IPage page, string name, string ip) {
        await page.GotoAsync($"{_fixture.BaseUrl}/systems/list");

        var list = new SystemsListPom(page);
        await list.AssertLoadedAsync();
        await list.AddSystemAsync(name);

        if (!page.Url.Contains($"/resources/systems/{name}", StringComparison.OrdinalIgnoreCase))
            await list.OpenSystemAsync(name);

        var card = new SystemCardPom(page);
        await card.AssertVisibleAsync(name);

        await card.BeginEditAsync(name);
        await card.IpInput(name).FillAsync(ip);
        await card.SaveAsync(name);

        await Assertions.Expect(card.IpValue(name)).ToContainTextAsync(ip);
    }

    private async Task DumpAsync(IPage page) {
        _output.WriteLine("TEST FAILED — Capturing diagnostics");
        _output.WriteLine($"Current URL: {page.Url}");

        var html = await page.ContentAsync();
        _output.WriteLine("==== DOM SNAPSHOT START ====");
        _output.WriteLine(html);
        _output.WriteLine("==== DOM SNAPSHOT END ====");
    }
}
