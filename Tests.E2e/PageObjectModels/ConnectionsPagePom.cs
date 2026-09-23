using Microsoft.Playwright;

namespace Tests.E2e.PageObjectModels;

/// <summary>
///     The global /connections page. It mounts PortConnectionModal with
///     TestIdPrefix="connections" — note that is the modal's base directly,
///     without the "-port-group" segment a resource card contributes.
/// </summary>
public class ConnectionsPagePom(IPage page) {
    private const string _modalBaseTestId = "connections";

    public ConnectionModalPom Modal => new(page, _modalBaseTestId);

    public ILocator Root
        => page.GetByTestId("connections-page-root");

    public ILocator AddButton
        => page.GetByTestId("connections-add-button");

    // -------------------------------------------------
    // High-Level Actions
    // -------------------------------------------------

    public async Task GotoAsync(string baseUrl) {
        await page.GotoAsync($"{baseUrl}/connections");
        await AssertLoadedAsync();
    }

    public async Task AssertLoadedAsync()
        => await Assertions.Expect(Root).ToBeVisibleAsync();

    public async Task OpenModalAsync() {
        await AddButton.ClickAsync();
        await Modal.AssertOpenAsync();
    }
}
