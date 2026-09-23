using Microsoft.Playwright;

namespace Tests.E2e.PageObjectModels;

public class SshExportPom(IPage page) {
    // -------------------------------------------------
    // Root
    // -------------------------------------------------

    public ILocator Page
        => page.GetByTestId("ssh-export-page");

    // -------------------------------------------------
    // Actions
    // -------------------------------------------------

    public ILocator GenerateButton
        => page.GetByTestId("generate-ssh-button");

    // -------------------------------------------------
    // Inputs
    // -------------------------------------------------

    public ILocator IncludeTagsInput
        => page.GetByTestId("ssh-include-tags-input");

    public ILocator DefaultUserInput
        => page.GetByTestId("ssh-default-user-input");

    public ILocator DefaultPortInput
        => page.GetByTestId("ssh-default-port-input");

    public ILocator DefaultIdentityInput
        => page.GetByTestId("ssh-default-identity-input");

    // -------------------------------------------------
    // Output
    // -------------------------------------------------

    public ILocator Output
        => page.GetByTestId("ssh-output");

    public ILocator WarningsContainer
        => page.GetByTestId("ssh-warnings");

    // -------------------------------------------------
    // High-Level Actions
    // -------------------------------------------------

    public async Task AssertVisibleAsync()
        => await Assertions.Expect(Page).ToBeVisibleAsync();

    public async Task SetIncludeTagsAsync(string value)
        => await IncludeTagsInput.FillAsync(value);

    public async Task SetDefaultUserAsync(string value)
        => await DefaultUserInput.FillAsync(value);

    public async Task SetDefaultPortAsync(string value)
        => await DefaultPortInput.FillAsync(value);

    public async Task SetDefaultIdentityAsync(string value)
        => await DefaultIdentityInput.FillAsync(value);

    public async Task GenerateAsync()
        => await GenerateButton.ClickAsync();

    public async Task<string> GetOutputTextAsync()
        => await Output.InputValueAsync();

    public async Task AssertOutputContainsAsync(string text)
        => await Assertions.Expect(Output).ToContainTextAsync(text);

    public async Task AssertNoWarningsAsync()
        => await Assertions.Expect(WarningsContainer).ToHaveCountAsync(0);
}
