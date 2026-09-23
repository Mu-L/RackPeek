using Microsoft.Playwright;

namespace Tests.E2e.PageObjectModels;

/// <summary>
///     The /labels/{LabelName} detail page — everything carrying one label key,
///     with that resource's value for it. Distinct from <see cref="LabelsPom" />,
///     which drives the label editor on a resource card.
/// </summary>
public class LabelPagePom(IPage page) {
    public ILocator Root
        => page.GetByTestId("label-page-root");

    public ILocator Title
        => page.GetByTestId("label-page-title");

    public ILocator List
        => page.GetByTestId("label-page-list");

    public ILocator Empty
        => page.GetByTestId("label-page-empty");

    public ILocator NoResults
        => page.GetByTestId("label-page-no-results");

    public ILocator Filter
        => page.GetByTestId("label-page-filter");

    public ILocator SortSelect
        => page.GetByTestId("label-page-sort");

    public ILocator SortDirectionButton
        => page.GetByTestId("label-page-sort-direction");

    public ILocator Item(string resourceName)
        => page.GetByTestId($"label-page-item-{resourceName.Replace(" ", "-")}");

    public ILocator ItemValue(string resourceName)
        => Item(resourceName).GetByTestId("label-page-item-value");

    // -------------------------------------------------
    // High-Level Actions
    // -------------------------------------------------

    public async Task GotoAsync(string baseUrl, string labelKey) {
        await page.GotoAsync($"{baseUrl}/labels/{Uri.EscapeDataString(labelKey)}");
        await AssertLoadedAsync();
    }

    public async Task AssertLoadedAsync()
        => await Assertions.Expect(Root).ToBeVisibleAsync();

    public async Task AssertTitleContainsAsync(string labelKey)
        => await Assertions.Expect(Title).ToContainTextAsync(labelKey);

    public async Task AssertListsAsync(string resourceName)
        => await Assertions.Expect(Item(resourceName)).ToBeVisibleAsync();

    public async Task AssertDoesNotListAsync(string resourceName)
        => await Assertions.Expect(Item(resourceName)).ToHaveCountAsync(0);

    /// <summary>
    ///     Asserts the resource's value for this label, plus the type the page
    ///     detected for it (TEXT / NUMBER / BOOL / DATE).
    /// </summary>
    public async Task AssertValueAsync(string resourceName, string value, string detectedType) {
        await Assertions.Expect(ItemValue(resourceName)).ToContainTextAsync(value);
        await Assertions.Expect(ItemValue(resourceName)).ToContainTextAsync(detectedType);
    }

    /// <summary>
    ///     The filter is a plain @bind, committing on change rather than input,
    ///     so the value needs an explicit blur — see DocsPom.SearchAsync.
    /// </summary>
    public async Task FilterAsync(string value) {
        await Filter.FillAsync(value);
        await Filter.BlurAsync();
    }

    public async Task SortByAsync(string option)
        => await SortSelect.SelectOptionAsync(option);

    public async Task ToggleSortDirectionAsync()
        => await SortDirectionButton.ClickAsync();

    public async Task AssertNoResultsAsync()
        => await Assertions.Expect(NoResults).ToBeVisibleAsync();

    /// <summary>
    ///     Names of the listed resources, in render order.
    /// </summary>
    public async Task<IReadOnlyList<string>> ListedOrderAsync() {
        var ids = await List.Locator("[data-testid^='label-page-item-']")
            .EvaluateAllAsync<string[]>(
                "els => els.map(e => e.getAttribute('data-testid'))");

        return ids
            .Where(id => id.StartsWith("label-page-item-", StringComparison.Ordinal))
            .Select(id => id["label-page-item-".Length..])
            .Where(name => name != "value")
            .ToList();
    }
}
