using Microsoft.Playwright;
using Tests.E2e.Infra;
using Tests.E2e.PageObjectModels;
using Xunit.Abstractions;

namespace Tests.E2e;

/// <summary>
///     Coverage for /tags/{TagName} and /labels/{LabelName} — the pages that
///     aggregate resources across kinds. The tag and label editors on the cards
///     were already covered; where those links lead was not.
///     Both pages read the whole config and tests share a container, so every
///     test uses a unique tag or label key to stay deterministic.
/// </summary>
public class TagAndLabelPageTests(
    PlaywrightFixture fixture,
    ITestOutputHelper output) : E2ETestBase(fixture, output) {
    private readonly PlaywrightFixture _fixture = fixture;
    private readonly ITestOutputHelper _output = output;

    private static string Unique(string prefix) => $"{prefix}{Guid.NewGuid():N}"[..14];

    // =============================================================
    // Tag page — aggregation across kinds
    // =============================================================

    [Fact]
    public async Task Tag_Page_Lists_Resources_Of_Every_Kind_Carrying_The_Tag() {
        (IBrowserContext context, IPage page) = await CreatePageAsync();

        var tag = Unique("e2etag");
        var serverName = Unique("e2e-tsv-");
        var systemName = Unique("e2e-tsy-");

        try {
            await CreateServerWithTagAsync(page, serverName, tag);
            await CreateSystemWithTagAsync(page, systemName, tag);

            var tagPage = new TagPagePom(page);
            await tagPage.GotoAsync(_fixture.BaseUrl, tag);

            await tagPage.AssertTitleContainsAsync(tag);

            // A hardware resource and a system, aggregated onto one page.
            await tagPage.AssertListsAsync(serverName);
            await tagPage.AssertListsAsync(systemName);
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
    // Tag page — click-through from a card, and back out again
    // =============================================================

    [Fact]
    public async Task User_Can_Click_A_Card_Tag_Through_To_The_Tag_Page_And_Back() {
        (IBrowserContext context, IPage page) = await CreatePageAsync();

        var tag = Unique("e2enav");
        var serverName = Unique("e2e-tnv-");

        try {
            await CreateServerWithTagAsync(page, serverName, tag);

            // Follow the tag chip on the card.
            var card = new ServerCardPom(page);
            await card.Tags.NavigateToTagAsync("server", tag);

            var tagPage = new TagPagePom(page);
            await tagPage.AssertLoadedAsync();
            await tagPage.AssertListsAsync(serverName);

            // And back out to the resource it points at.
            await tagPage.OpenResourceAsync(serverName);
            await card.AssertVisibleAsync(serverName);
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
    public async Task Tag_Page_Reports_An_Unused_Tag_As_Empty() {
        (IBrowserContext context, IPage page) = await CreatePageAsync();

        try {
            var tagPage = new TagPagePom(page);
            await tagPage.GotoAsync(_fixture.BaseUrl, Unique("e2enone"));

            await tagPage.AssertEmptyAsync();
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
    // Label page — values and detected types
    // =============================================================

    [Fact]
    public async Task Label_Page_Shows_Each_Resource_Value_And_Its_Detected_Type() {
        (IBrowserContext context, IPage page) = await CreatePageAsync();

        var labelKey = Unique("e2elbl");
        var textServer = Unique("e2e-ltx-");
        var numberServer = Unique("e2e-lnm-");

        try {
            await CreateServerWithLabelAsync(page, textServer, labelKey, "production");
            await CreateServerWithLabelAsync(page, numberServer, labelKey, "42");

            var labelPage = new LabelPagePom(page);
            await labelPage.GotoAsync(_fixture.BaseUrl, labelKey);

            await labelPage.AssertTitleContainsAsync(labelKey);

            // The page classifies each value, not just displays it.
            await labelPage.AssertValueAsync(textServer, "production", "TEXT");
            await labelPage.AssertValueAsync(numberServer, "42", "NUMBER");
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
    // Label page — filtering
    // =============================================================

    [Fact]
    public async Task Label_Page_Filter_Narrows_The_List_And_Can_Match_Nothing() {
        (IBrowserContext context, IPage page) = await CreatePageAsync();

        var labelKey = Unique("e2efil");
        var keptServer = Unique("e2e-lkp-");
        var filteredServer = Unique("e2e-lfl-");

        try {
            await CreateServerWithLabelAsync(page, keptServer, labelKey, "alpha");
            await CreateServerWithLabelAsync(page, filteredServer, labelKey, "beta");

            var labelPage = new LabelPagePom(page);
            await labelPage.GotoAsync(_fixture.BaseUrl, labelKey);

            await labelPage.AssertListsAsync(keptServer);
            await labelPage.AssertListsAsync(filteredServer);

            // The filter matches on the label value as well as the name.
            await labelPage.FilterAsync("alpha");
            await labelPage.AssertListsAsync(keptServer);
            await labelPage.AssertDoesNotListAsync(filteredServer);

            // Nothing matching falls through to its own state, not an empty list.
            await labelPage.FilterAsync(Unique("nomatch"));
            await labelPage.AssertNoResultsAsync();
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
    // Label page — sort direction
    // =============================================================

    [Fact]
    public async Task Label_Page_Sort_Direction_Reverses_The_List() {
        (IBrowserContext context, IPage page) = await CreatePageAsync();

        var labelKey = Unique("e2esrt");
        var lowServer = Unique("e2e-slo-");
        var highServer = Unique("e2e-shi-");

        try {
            // Numeric values so the default "Value" sort is unambiguous.
            await CreateServerWithLabelAsync(page, lowServer, labelKey, "1");
            await CreateServerWithLabelAsync(page, highServer, labelKey, "2");

            var labelPage = new LabelPagePom(page);
            await labelPage.GotoAsync(_fixture.BaseUrl, labelKey);

            IReadOnlyList<string> ascending = await labelPage.ListedOrderAsync();
            Assert.Equal(new[] { lowServer, highServer }, ascending);

            await labelPage.ToggleSortDirectionAsync();

            IReadOnlyList<string> descending = await labelPage.ListedOrderAsync();
            Assert.Equal(new[] { highServer, lowServer }, descending);
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

    private async Task CreateServerAsync(IPage page, string name) {
        await page.GotoAsync($"{_fixture.BaseUrl}/servers/list");

        var list = new ServersListPom(page);
        await list.AssertLoadedAsync();
        await list.AddServerAsync(name);

        if (!page.Url.Contains($"/resources/hardware/{name}", StringComparison.OrdinalIgnoreCase))
            await list.OpenServerAsync(name);

        var card = new ServerCardPom(page);
        await card.AssertVisibleAsync(name);
    }

    private async Task CreateServerWithTagAsync(IPage page, string name, string tag) {
        await CreateServerAsync(page, name);

        var card = new ServerCardPom(page);
        await card.Tags.AddTagsAsync("server", tag);
        await card.Tags.AssertTagVisibleAsync("server", tag);
    }

    private async Task CreateServerWithLabelAsync(IPage page, string name, string key, string value) {
        await CreateServerAsync(page, name);

        var card = new ServerCardPom(page);
        await card.Labels.AddLabelAsync("server", key, value);
        await card.Labels.AssertLabelVisibleAsync("server", key);
    }

    private async Task CreateSystemWithTagAsync(IPage page, string name, string tag) {
        await page.GotoAsync($"{_fixture.BaseUrl}/systems/list");

        var list = new SystemsListPom(page);
        await list.AssertLoadedAsync();
        await list.AddSystemAsync(name);

        if (!page.Url.Contains($"/resources/systems/{name}", StringComparison.OrdinalIgnoreCase))
            await list.OpenSystemAsync(name);

        var card = new SystemCardPom(page);
        await card.AssertVisibleAsync(name);

        await card.Tags.AddTagsAsync("system", tag);
        await card.Tags.AssertTagVisibleAsync("system", tag);
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
