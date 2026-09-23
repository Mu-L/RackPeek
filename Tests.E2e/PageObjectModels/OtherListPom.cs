using Microsoft.Playwright;

namespace Tests.E2e.PageObjectModels;

public class OtherListPom(IPage page) {
    public AddResourceComponent AddOther => new(page, "other");

    public ILocator PageRoot => page.GetByTestId("other-page-root");
    public ILocator PageTitle => page.GetByTestId("other-page-title");

    public ILocator Loading => page.GetByTestId("other-loading");
    public ILocator EmptyState => page.GetByTestId("other-empty");
    public ILocator OtherList => page.GetByTestId("other-list");

    public ILocator AddSection => page.GetByTestId("other-add-section");

    // Must match AddResourceComponent test IDs
    public ILocator AddInput => page.GetByTestId("add-other-input");
    public ILocator AddButton => page.GetByTestId("add-other-button");

    // -------------------------------------------------
    // Dynamic Other Items
    // -------------------------------------------------

    public ILocator OtherItem(string name) => page.GetByTestId($"other-item-{Sanitize(name)}");

    public ILocator OpenLink(string name) {
        return OtherItem(name)
            .GetByTestId($"other-item-{Sanitize(name)}-link");
    }

    public ILocator EditButton(string name) {
        return OtherItem(name)
            .GetByTestId("edit-other-button");
    }

    public ILocator SaveButton(string name) {
        return OtherItem(name)
            .GetByTestId("save-other-button");
    }

    public ILocator CancelButton(string name) {
        return OtherItem(name)
            .GetByTestId("cancel-other-button");
    }

    public ILocator RenameButton(string name) {
        return OtherItem(name)
            .GetByTestId("rename-other-button");
    }

    public ILocator CloneButton(string name) {
        return OtherItem(name)
            .GetByTestId("clone-other-button");
    }

    public ILocator DeleteButton(string name) {
        return OtherItem(name)
            .GetByTestId("delete-other-button");
    }

    // -------------------------------------------------
    // Navigation
    // -------------------------------------------------

    public async Task GotoAsync(string baseUrl) {
        await page.GotoAsync($"{baseUrl}/other/list");
        await AssertLoadedAsync();
    }

    public async Task AssertLoadedAsync() {
        await Assertions.Expect(PageRoot).ToBeVisibleAsync();
        await Assertions.Expect(PageTitle).ToBeVisibleAsync();
    }

    public async Task WaitForListAsync() => await Assertions.Expect(OtherList).ToBeVisibleAsync();

    // -------------------------------------------------
    // Actions
    // -------------------------------------------------

    public async Task AddOtherAsync(string name) {
        await AddOther.AddAsync(name);
        await Assertions.Expect(OtherItem(name))
            .ToBeVisibleAsync();
    }

    public async Task DeleteOtherAsync(string name) {
        await DeleteButton(name).ClickAsync();
        await page.GetByTestId("Other-confirm-modal-confirm").ClickAsync();

        await Assertions.Expect(OtherItem(name))
            .Not.ToBeVisibleAsync();
    }

    public async Task OpenOtherAsync(string name) {
        await OpenLink(name).ClickAsync();
        await page.WaitForURLAsync($"**/resources/hardware/{name}");
    }

    public async Task AssertOtherExists(string name) {
        await Assertions.Expect(OtherItem(name))
            .ToBeVisibleAsync();
    }

    public async Task AssertOtherDoesNotExist(string name) {
        await Assertions.Expect(OtherItem(name))
            .Not.ToBeVisibleAsync();
    }

    private static string Sanitize(string value) => value.Replace(" ", "-");
}
