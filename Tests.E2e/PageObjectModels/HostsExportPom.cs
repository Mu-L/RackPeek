using Microsoft.Playwright;

namespace Tests.E2e.PageObjectModels;

public class HostsExportPom(IPage page) {
    // -------------------------------------------------
    // Root
    // -------------------------------------------------

    public ILocator Page
        => page.GetByTestId("hosts-export-page");

    // -------------------------------------------------
    // Actions
    // -------------------------------------------------

    public ILocator GenerateButton
        => page.GetByTestId("generate-hosts-button");

    // -------------------------------------------------
    // Inputs
    // -------------------------------------------------

    public ILocator IncludeTagsInput
        => page.GetByTestId("hosts-include-tags-input");

    public ILocator DomainSuffixInput
        => page.GetByTestId("hosts-domain-suffix-input");

    public ILocator IncludeLocalhostCheckbox
        => page.GetByTestId("hosts-include-localhost-checkbox");

    // -------------------------------------------------
    // Output
    // -------------------------------------------------

    public ILocator Output
        => page.GetByTestId("hosts-output");

    public ILocator WarningsContainer
        => page.GetByTestId("hosts-warnings");

    // -------------------------------------------------
    // High-Level Actions
    // -------------------------------------------------

    public async Task AssertVisibleAsync()
        => await Assertions.Expect(Page).ToBeVisibleAsync();

    public async Task SetIncludeTagsAsync(string value)
        => await IncludeTagsInput.FillAsync(value);

    public async Task SetDomainSuffixAsync(string value)
        => await DomainSuffixInput.FillAsync(value);

    public async Task SetIncludeLocalhostAsync(bool value)
        => await IncludeLocalhostCheckbox.SetCheckedAsync(value);

    public async Task GenerateAsync()
        => await GenerateButton.ClickAsync();

    public async Task<string> GetOutputTextAsync()
        => await Output.InputValueAsync();

    public async Task AssertOutputContainsAsync(string text)
        => await Assertions.Expect(Output).ToContainTextAsync(text);

    public async Task AssertOutputDoesNotContainAsync(string text)
        => await Assertions.Expect(Output).Not.ToContainTextAsync(text);

    public async Task AssertNoWarningsAsync()
        => await Assertions.Expect(WarningsContainer).ToHaveCountAsync(0);
}
