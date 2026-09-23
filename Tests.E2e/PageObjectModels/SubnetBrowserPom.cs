using Microsoft.Playwright;

namespace Tests.E2e.PageObjectModels;

public class SubnetBrowserPom(IPage page) {
    // -------------------------------------------------
    // Root
    // -------------------------------------------------

    public ILocator Root
        => page.GetByTestId("subnet-browser-root");

    public ILocator Title
        => page.GetByTestId("subnet-browser-title");

    public ILocator Filter
        => page.GetByTestId("subnet-browser-filter");

    // -------------------------------------------------
    // States
    // -------------------------------------------------

    public ILocator Loading
        => page.GetByTestId("subnet-browser-loading");

    public ILocator Empty
        => page.GetByTestId("subnet-browser-empty");

    public ILocator List
        => page.GetByTestId("subnet-browser-list");

    /// <summary>
    ///     A subnet group, addressed by its /24 key. The page groups on the
    ///     first three octets, so 10.42.7.21 lands in the "10.42.7.x" group.
    /// </summary>
    public ILocator Group(string subnetKey)
        => page.GetByTestId($"subnet-group-{subnetKey.Replace('.', '-')}");

    // -------------------------------------------------
    // High-Level Actions
    // -------------------------------------------------

    public async Task AssertLoadedAsync() {
        await Assertions.Expect(Root).ToBeVisibleAsync();
        await Assertions.Expect(Title).ToBeVisibleAsync();
    }

    /// <summary>
    ///     The filter binds on oninput, so no explicit blur is needed here.
    /// </summary>
    public async Task FilterAsync(string value)
        => await Filter.FillAsync(value);

    public async Task AssertGroupVisibleAsync(string subnetKey)
        => await Assertions.Expect(Group(subnetKey)).ToBeVisibleAsync();

    public async Task AssertGroupContainsAsync(string subnetKey, string text)
        => await Assertions.Expect(Group(subnetKey)).ToContainTextAsync(text);

    public async Task AssertEmptyAsync()
        => await Assertions.Expect(Empty).ToBeVisibleAsync();
}
