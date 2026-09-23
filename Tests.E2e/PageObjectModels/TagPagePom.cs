using Microsoft.Playwright;

namespace Tests.E2e.PageObjectModels;

/// <summary>
///     The /tags/{TagName} detail page — everything carrying one tag.
///     Distinct from <see cref="TagsPom" />, which drives the tag editor on a
///     resource card.
/// </summary>
public class TagPagePom(IPage page) {
    public ILocator Root
        => page.GetByTestId("tag-page-root");

    public ILocator Title
        => page.GetByTestId("tag-page-title");

    public ILocator List
        => page.GetByTestId("tag-page-list");

    public ILocator Empty
        => page.GetByTestId("tag-page-empty");

    public ILocator Item(string resourceName)
        => page.GetByTestId($"tag-page-item-{resourceName.Replace(" ", "-")}");

    // -------------------------------------------------
    // High-Level Actions
    // -------------------------------------------------

    public async Task GotoAsync(string baseUrl, string tag) {
        await page.GotoAsync($"{baseUrl}/tags/{Uri.EscapeDataString(tag)}");
        await AssertLoadedAsync();
    }

    public async Task AssertLoadedAsync()
        => await Assertions.Expect(Root).ToBeVisibleAsync();

    public async Task AssertTitleContainsAsync(string tag)
        => await Assertions.Expect(Title).ToContainTextAsync(tag);

    public async Task AssertListsAsync(string resourceName)
        => await Assertions.Expect(Item(resourceName)).ToBeVisibleAsync();

    public async Task AssertDoesNotListAsync(string resourceName)
        => await Assertions.Expect(Item(resourceName)).ToHaveCountAsync(0);

    public async Task AssertEmptyAsync()
        => await Assertions.Expect(Empty).ToBeVisibleAsync();

    /// <summary>
    ///     Follows the link on a listed resource through to its card.
    /// </summary>
    public async Task OpenResourceAsync(string resourceName) {
        await Item(resourceName).GetByRole(AriaRole.Link).First.ClickAsync();
        await page.WaitForURLAsync($"**/resources/**/{resourceName}");
    }
}
