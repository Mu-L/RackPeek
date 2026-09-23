using Microsoft.Playwright;
using Tests.E2e.Infra;
using Tests.E2e.PageObjectModels;
using Xunit.Abstractions;

namespace Tests.E2e;

/// <summary>
///     Coverage for the global /connections page — previously the only page in
///     the app with no test ids at all, and the only route from which a
///     connection can be made without going through a resource card.
///     Fixtures are seeded through the YAML import rather than by clicking port
///     groups into existence, which keeps the setup to one step.
/// </summary>
public class ConnectionsPageTests(
    PlaywrightFixture fixture,
    ITestOutputHelper output) : E2ETestBase(fixture, output) {
    private readonly PlaywrightFixture _fixture = fixture;
    private readonly ITestOutputHelper _output = output;

    private const string _portType = "rj45";
    private const string _portSpeed = "1";
    private const int _portCount = 4;

    private static string TwoSwitchesNoConnection(string switchA, string switchB) =>
        $"""
         version: 3
         resources:
           - kind: Switch
             name: {switchA}
             ports:
               - type: {_portType}
                 speed: {_portSpeed}
                 count: {_portCount}
           - kind: Switch
             name: {switchB}
             ports:
               - type: {_portType}
                 speed: {_portSpeed}
                 count: {_portCount}
         """;

    // =============================================================
    // Creating a connection from the global page
    // =============================================================

    [Fact]
    public async Task User_Can_Connect_Two_Resources_From_The_Connections_Page() {
        (IBrowserContext context, IPage page) = await CreatePageAsync();

        var switchA = $"e2e-cxa-{Guid.NewGuid():N}"[..14];
        var switchB = $"e2e-cxb-{Guid.NewGuid():N}"[..14];

        try {
            await SeedSwitchesAsync(page, switchA, switchB);

            var connections = new ConnectionsPagePom(page);
            await connections.GotoAsync(_fixture.BaseUrl);
            await connections.OpenModalAsync();

            await connections.Modal.CreateConnectionAsync(
                switchA,
                ConnectionModalPom.GroupLabel(_portType, _portSpeed, _portCount),
                ConnectionModalPom.PortLabel(1),
                switchB,
                ConnectionModalPom.GroupLabel(_portType, _portSpeed, _portCount),
                ConnectionModalPom.PortLabel(1));

            // Submitting closes the modal.
            await connections.Modal.AssertClosedAsync();

            // The saved config is the source of truth.
            await page.GotoAsync($"{_fixture.BaseUrl}/yaml");

            ILocator yaml = page.GetByTestId("yaml-file-content");
            await Assertions.Expect(yaml).ToBeVisibleAsync();
            await Assertions.Expect(yaml).ToContainTextAsync("connections:");
            await Assertions.Expect(yaml).ToContainTextAsync(switchA);
            await Assertions.Expect(yaml).ToContainTextAsync(switchB);
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
    // Connection labels survive the round trip
    // =============================================================

    [Fact]
    public async Task A_Connection_Label_Is_Persisted_With_The_Connection() {
        (IBrowserContext context, IPage page) = await CreatePageAsync();

        var switchA = $"e2e-cla-{Guid.NewGuid():N}"[..14];
        var switchB = $"e2e-clb-{Guid.NewGuid():N}"[..14];
        var label = $"uplink-{Guid.NewGuid():N}"[..14];

        try {
            await SeedSwitchesAsync(page, switchA, switchB);

            var connections = new ConnectionsPagePom(page);
            await connections.GotoAsync(_fixture.BaseUrl);
            await connections.OpenModalAsync();

            await connections.Modal.CreateConnectionAsync(
                switchA,
                ConnectionModalPom.GroupLabel(_portType, _portSpeed, _portCount),
                ConnectionModalPom.PortLabel(2),
                switchB,
                ConnectionModalPom.GroupLabel(_portType, _portSpeed, _portCount),
                ConnectionModalPom.PortLabel(2),
                label);

            await connections.Modal.AssertClosedAsync();

            await page.GotoAsync($"{_fixture.BaseUrl}/yaml");

            ILocator yaml = page.GetByTestId("yaml-file-content");
            await Assertions.Expect(yaml).ToBeVisibleAsync();
            await Assertions.Expect(yaml).ToContainTextAsync(label);
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
    // Opening and dismissing the modal
    // =============================================================

    [Fact]
    public async Task Connections_Page_Opens_The_Connection_Modal() {
        (IBrowserContext context, IPage page) = await CreatePageAsync();

        try {
            var connections = new ConnectionsPagePom(page);
            await connections.GotoAsync(_fixture.BaseUrl);

            // The modal is not mounted until asked for.
            await connections.Modal.AssertClosedAsync();

            await connections.OpenModalAsync();
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

    private async Task SeedSwitchesAsync(IPage page, string switchA, string switchB) {
        var import = new YamlImportPom(page);
        await import.GotoAsync(_fixture.BaseUrl);
        await import.PasteAsync(TwoSwitchesNoConnection(switchA, switchB));
        await import.AssertNoErrorAsync();
        await import.ApplyAsync();
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
