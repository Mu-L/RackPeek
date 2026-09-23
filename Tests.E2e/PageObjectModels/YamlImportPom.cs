using Microsoft.Playwright;

namespace Tests.E2e.PageObjectModels;

public class YamlImportPom(IPage page) {
    // -------------------------------------------------
    // Input
    // -------------------------------------------------

    /// <summary>
    ///     The YAML textarea carries no test id, so it is addressed by its
    ///     placeholder. It binds on oninput and recomputes the diff after each
    ///     change, so filling it is enough to render the preview.
    /// </summary>
    public ILocator Input
        => page.GetByPlaceholder("Paste YAML here...");

    public ILocator ApplyButton
        => page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Apply" });

    // -------------------------------------------------
    // Errors
    // -------------------------------------------------

    public ILocator Error
        => page.GetByTestId("yaml-import-error");

    public ILocator ErrorSnippet
        => page.GetByTestId("yaml-import-error-snippet");

    // -------------------------------------------------
    // Connections preview (issue #308)
    // -------------------------------------------------

    public ILocator ConnectionsSummary
        => page.GetByTestId("import-connections-summary");

    public ILocator ConnectionsAdded
        => page.GetByTestId("import-connection-added");

    public ILocator ConnectionsRemoved
        => page.GetByTestId("import-connection-removed");

    // -------------------------------------------------
    // High-Level Actions
    // -------------------------------------------------

    public async Task GotoAsync(string baseUrl) {
        await page.GotoAsync($"{baseUrl}/yaml/import");
        await Assertions.Expect(Input).ToBeVisibleAsync();
    }

    public async Task PasteAsync(string yaml)
        => await Input.FillAsync(yaml);

    public async Task ApplyAsync() {
        await Assertions.Expect(ApplyButton).ToBeEnabledAsync();
        await ApplyButton.ClickAsync();
    }

    public async Task AssertNoErrorAsync()
        => await Assertions.Expect(Error).ToHaveCountAsync(0);

    public async Task AssertErrorShownAsync()
        => await Assertions.Expect(Error).ToBeVisibleAsync();

    public async Task AssertConnectionsSummaryVisibleAsync()
        => await Assertions.Expect(ConnectionsSummary).ToBeVisibleAsync();

    public async Task AssertConnectionAddedContainsAsync(string text)
        => await Assertions.Expect(ConnectionsAdded.First).ToContainTextAsync(text);
}
