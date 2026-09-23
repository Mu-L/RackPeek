using Microsoft.Playwright;

namespace Tests.E2e.PageObjectModels;

public class DocsPom(IPage page) {
    // -------------------------------------------------
    // Root
    // -------------------------------------------------

    public ILocator Viewer
        => page.GetByTestId("docs-viewer");

    public ILocator HomeLink
        => page.GetByTestId("docs-home-link");

    public ILocator SearchInput
        => page.GetByTestId("docs-search-input");

    // -------------------------------------------------
    // Index
    // -------------------------------------------------

    /// <summary>
    ///     Every sidebar entry. Matched on the test-id prefix so the assertions
    ///     survive docs being added to or removed from docs-index.json.
    /// </summary>
    public ILocator IndexLinks
        => page.Locator("[data-testid^='docs-index-link-']");

    /// <summary>
    ///     A single sidebar entry, addressed by its source file name
    ///     (e.g. "overview.md" -> docs-index-link-overview-md).
    /// </summary>
    public ILocator IndexLink(string docFileName)
        => page.GetByTestId($"docs-index-link-{Sanitize(docFileName)}");

    private static string Sanitize(string value)
        => value.Replace(" ", "-")
            .Replace("/", "-")
            .Replace("\\", "-")
            .Replace(".", "-");

    // -------------------------------------------------
    // High-Level Actions
    // -------------------------------------------------

    public async Task AssertLoadedAsync() {
        await Assertions.Expect(Viewer).ToBeVisibleAsync();
        await Assertions.Expect(SearchInput).ToBeVisibleAsync();
    }

    public async Task OpenDocAsync(string docFileName) {
        await IndexLink(docFileName).ClickAsync();
        await page.WaitForURLAsync($"**/docs/{docFileName.Replace(".md", "")}");
    }

    /// <summary>
    ///     The search box is a plain Blazor @bind, which updates on the change
    ///     event rather than on input. Playwright's FillAsync only raises input,
    ///     so the value must be committed with an explicit blur — elsewhere in
    ///     the app a Generate button happens to do that as a side effect.
    /// </summary>
    public async Task SearchAsync(string filter) {
        await SearchInput.FillAsync(filter);
        await SearchInput.BlurAsync();
    }

    public async Task AssertContentContainsAsync(string text)
        => await Assertions.Expect(Viewer).ToContainTextAsync(text);

    /// <summary>
    ///     Guards issue #304: when the Blazor Server host cannot read the docs
    ///     assets it renders the "document not found" placeholder rather than
    ///     failing outright, so a passing page load is not on its own proof the
    ///     content resolved.
    /// </summary>
    public async Task AssertContentResolvedAsync() {
        await Assertions.Expect(Viewer).Not.ToContainTextAsync("document not found");
        await Assertions.Expect(Viewer).Not.ToContainTextAsync("loading documentation…");
    }

    public async Task AssertIndexResolvedAsync() {
        await Assertions.Expect(Viewer).Not.ToContainTextAsync("docs index not found");
        await Assertions.Expect(IndexLinks.First).ToBeVisibleAsync();
    }
}
