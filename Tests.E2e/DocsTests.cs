using Microsoft.Playwright;
using Tests.E2e.Infra;
using Tests.E2e.PageObjectModels;
using Xunit.Abstractions;

namespace Tests.E2e;

/// <summary>
///     Coverage for the /docs knowledge base. The Blazor Server host reads the
///     docs assets off disk via StaticWebAssetDocsContentProvider; fetching them
///     over HTTP breaks behind a reverse proxy (issue #304), and the failure is
///     silent — the page still renders, just with a "document not found"
///     placeholder. These tests assert on resolved content for that reason.
/// </summary>
public class DocsTests(
    PlaywrightFixture fixture,
    ITestOutputHelper output) : E2ETestBase(fixture, output) {
    private readonly PlaywrightFixture _fixture = fixture;
    private readonly ITestOutputHelper _output = output;

    // =============================================================
    // Overview renders (issue #304 regression)
    // =============================================================

    [Fact]
    public async Task Docs_Overview_Renders_Content_Not_A_Not_Found_Placeholder() {
        (IBrowserContext context, IPage page) = await CreatePageAsync();

        try {
            await page.GotoAsync($"{_fixture.BaseUrl}/docs");

            var docs = new DocsPom(page);
            await docs.AssertLoadedAsync();

            // The index and the default overview document must both resolve.
            await docs.AssertIndexResolvedAsync();
            await docs.AssertContentResolvedAsync();
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
    // Deep link renders
    // =============================================================

    [Fact]
    public async Task User_Can_Deep_Link_Straight_To_A_Docs_Page() {
        (IBrowserContext context, IPage page) = await CreatePageAsync();

        try {
            // Navigating directly (rather than clicking through) is the path a
            // bookmark or an external link takes.
            await page.GotoAsync($"{_fixture.BaseUrl}/docs/ssh-config-export");

            var docs = new DocsPom(page);
            await docs.AssertLoadedAsync();
            await docs.AssertContentResolvedAsync();
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
    // Navigation between docs
    // =============================================================

    [Fact]
    public async Task User_Can_Navigate_Between_Docs_From_The_Index() {
        (IBrowserContext context, IPage page) = await CreatePageAsync();

        try {
            await page.GotoAsync($"{_fixture.BaseUrl}/docs");

            var docs = new DocsPom(page);
            await docs.AssertLoadedAsync();
            await docs.AssertIndexResolvedAsync();

            await docs.OpenDocAsync("install-guide.md");
            await docs.AssertContentResolvedAsync();

            // A second hop exercises OnParametersSetAsync re-fetching for an
            // already-initialised component, not just the first render.
            await docs.OpenDocAsync("versioning.md");
            await docs.AssertContentResolvedAsync();

            // Back to the overview via the header link.
            await docs.HomeLink.ClickAsync();
            await page.WaitForURLAsync("**/docs");
            await docs.AssertContentResolvedAsync();
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
    // Sidebar search
    // =============================================================

    [Fact]
    public async Task Docs_Search_Filters_The_Index() {
        (IBrowserContext context, IPage page) = await CreatePageAsync();

        try {
            await page.GotoAsync($"{_fixture.BaseUrl}/docs");

            var docs = new DocsPom(page);
            await docs.AssertLoadedAsync();
            await docs.AssertIndexResolvedAsync();

            var totalCount = await docs.IndexLinks.CountAsync();
            Assert.True(totalCount > 1, $"Expected a multi-entry docs index, found {totalCount}.");

            await docs.SearchAsync("ansible");

            // Only the ansible guide should survive the filter.
            await Assertions.Expect(docs.IndexLink("ansible-generator-guide.md")).ToBeVisibleAsync();
            await Assertions.Expect(docs.IndexLink("versioning.md")).ToHaveCountAsync(0);

            // Clearing restores the full index.
            await docs.SearchAsync("");
            await Assertions.Expect(docs.IndexLinks).ToHaveCountAsync(totalCount);
        }
        catch (Exception) {
            await DumpAsync(page);
            throw;
        }
        finally {
            await context.CloseAsync();
        }
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
