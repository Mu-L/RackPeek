using Microsoft.Playwright;
using Tests.E2e.Infra;
using Tests.E2e.PageObjectModels;
using Xunit.Abstractions;

namespace Tests.E2e;

/// <summary>
///     Coverage for the /ssh/export and /hosts/export generators. Both render
///     from whatever is in the config, so each test creates the system it
///     asserts on rather than relying on seed data — the container starts with
///     an empty config volume.
///     Kept in one class so both pages share a single container.
/// </summary>
public class ExportTests(
    PlaywrightFixture fixture,
    ITestOutputHelper output) : E2ETestBase(fixture, output) {
    private readonly PlaywrightFixture _fixture = fixture;
    private readonly ITestOutputHelper _output = output;

    // =============================================================
    // Hosts export — localhost defaults toggle
    // =============================================================

    [Fact]
    public async Task Hosts_Export_Honours_The_Localhost_Defaults_Toggle() {
        (IBrowserContext context, IPage page) = await CreatePageAsync();

        try {
            await page.GotoAsync($"{_fixture.BaseUrl}/hosts/export");

            var hosts = new HostsExportPom(page);
            await hosts.AssertVisibleAsync();

            // Defaults on: the loopback block is emitted.
            await hosts.SetIncludeLocalhostAsync(true);
            await hosts.GenerateAsync();
            await hosts.AssertOutputContainsAsync("127.0.0.1");
            await hosts.AssertNoWarningsAsync();

            // Defaults off: it is not.
            await hosts.SetIncludeLocalhostAsync(false);
            await hosts.GenerateAsync();
            await hosts.AssertOutputDoesNotContainAsync("127.0.0.1");
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
    // Hosts export — a real system reaches the output
    // =============================================================

    [Fact]
    public async Task Hosts_Export_Emits_An_Entry_For_A_System_With_An_Ip() {
        (IBrowserContext context, IPage page) = await CreatePageAsync();
        var name = $"e2e-hx-{Guid.NewGuid():N}"[..14];
        const string ip = "10.42.7.21";

        try {
            await CreateSystemWithIpAsync(page, name, ip);

            await page.GotoAsync($"{_fixture.BaseUrl}/hosts/export");

            var hosts = new HostsExportPom(page);
            await hosts.AssertVisibleAsync();

            await hosts.SetDomainSuffixAsync("home.local");
            await hosts.GenerateAsync();

            await hosts.AssertOutputContainsAsync(ip);
            await hosts.AssertOutputContainsAsync(name);
            await hosts.AssertOutputContainsAsync($"{name}.home.local");
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
    // SSH export — a real system reaches the output
    // =============================================================

    [Fact]
    public async Task Ssh_Export_Emits_A_Host_Block_For_A_System_With_An_Ip() {
        (IBrowserContext context, IPage page) = await CreatePageAsync();
        var name = $"e2e-sx-{Guid.NewGuid():N}"[..14];
        const string ip = "10.42.7.22";

        try {
            await CreateSystemWithIpAsync(page, name, ip);

            await page.GotoAsync($"{_fixture.BaseUrl}/ssh/export");

            var ssh = new SshExportPom(page);
            await ssh.AssertVisibleAsync();

            await ssh.SetDefaultUserAsync("ansible");
            await ssh.SetDefaultPortAsync("2222");
            await ssh.SetDefaultIdentityAsync("~/.ssh/id_ed25519");
            await ssh.GenerateAsync();

            await ssh.AssertOutputContainsAsync($"Host {name}");
            await ssh.AssertOutputContainsAsync($"HostName {ip}");
            await ssh.AssertOutputContainsAsync("User ansible");
            await ssh.AssertOutputContainsAsync("Port 2222");
            await ssh.AssertOutputContainsAsync("IdentityFile ~/.ssh/id_ed25519");
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
