using Microsoft.Playwright;

namespace Tests.E2e.PageObjectModels;

public class VisualisePom(IPage page) {
    // -------------------------------------------------
    // Root
    // -------------------------------------------------

    public ILocator Root
        => page.GetByTestId("visualise-page-root");

    // -------------------------------------------------
    // Tabs
    // -------------------------------------------------

    public ILocator TopologyTab
        => page.GetByTestId("visualise-tab-topology");

    public ILocator LogicalTab
        => page.GetByTestId("visualise-tab-logical");

    // -------------------------------------------------
    // Exports
    // -------------------------------------------------

    public ILocator ExportPngButton
        => page.GetByTestId("visualise-export-png");

    public ILocator ExportSourceButton
        => page.GetByTestId("visualise-export-source");

    // -------------------------------------------------
    // High-Level Actions
    // -------------------------------------------------

    public async Task AssertLoadedAsync()
        => await Assertions.Expect(Root).ToBeVisibleAsync();

    public async Task SelectTopologyAsync() {
        await TopologyTab.ClickAsync();
        await page.WaitForURLAsync("**/visualise/topology");
    }

    public async Task SelectLogicalAsync() {
        await LogicalTab.ClickAsync();
        await page.WaitForURLAsync("**/visualise/logical");
    }

    public async Task AssertExportsEnabledAsync() {
        await Assertions.Expect(ExportPngButton).ToBeEnabledAsync();
        await Assertions.Expect(ExportSourceButton).ToBeEnabledAsync();
    }
}
