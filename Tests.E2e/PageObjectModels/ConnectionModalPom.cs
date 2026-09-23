using Microsoft.Playwright;

namespace Tests.E2e.PageObjectModels;

/// <summary>
///     Drives PortConnectionModal, wherever it is mounted.
///     The modal builds its test ids from its TestIdPrefix parameter, which
///     differs by host: a resource card passes "{kind}-ports-port-group", while
///     the global /connections page passes plain "connections". This POM is
///     keyed on that base so both can share one implementation.
/// </summary>
public class ConnectionModalPom(IPage page, string baseTestId) {
    private string Id(string suffix) => $"{baseTestId}-connection-modal-{suffix}";

    public ILocator Container => page.GetByTestId(Id("container"));

    public ILocator ResourceASelect => page.GetByTestId(Id("resource-a"));
    public ILocator GroupASelect => page.GetByTestId(Id("group-a"));
    public ILocator PortASelect => page.GetByTestId(Id("port-a"));

    public ILocator ResourceBSelect => page.GetByTestId(Id("resource-b"));
    public ILocator GroupBSelect => page.GetByTestId(Id("group-b"));
    public ILocator PortBSelect => page.GetByTestId(Id("port-b"));

    public ILocator LabelInput => page.GetByTestId(Id("label"));
    public ILocator SubmitButton => page.GetByTestId(Id("submit"));
    public ILocator CancelButton => page.GetByTestId(Id("cancel"));

    // -------------------------------------------------
    // Assertions
    // -------------------------------------------------

    public async Task AssertOpenAsync()
        => await Assertions.Expect(Container).ToBeVisibleAsync();

    /// <summary>
    ///     The modal is wrapped in an @if, so when closed it is absent from the
    ///     DOM rather than merely hidden.
    /// </summary>
    public async Task AssertClosedAsync()
        => await Assertions.Expect(Container).ToHaveCountAsync(0);

    // -------------------------------------------------
    // Actions
    // -------------------------------------------------

    public async Task CreateConnectionAsync(
        string resourceA,
        string groupA,
        string portA,
        string resourceB,
        string groupB,
        string portB,
        string? label = null) {
        await ResourceASelect.SelectOptionAsync(new SelectOptionValue { Label = resourceA });
        await GroupASelect.SelectOptionAsync(new SelectOptionValue { Label = groupA });
        await PortASelect.SelectOptionAsync(new SelectOptionValue { Label = portA });

        await ResourceBSelect.SelectOptionAsync(new SelectOptionValue { Label = resourceB });
        await GroupBSelect.SelectOptionAsync(new SelectOptionValue { Label = groupB });
        await PortBSelect.SelectOptionAsync(new SelectOptionValue { Label = portB });

        if (label is not null)
            await LabelInput.FillAsync(label);

        await SubmitButton.ClickAsync();
    }

    public async Task CancelAsync()
        => await CancelButton.ClickAsync();

    // -------------------------------------------------
    // Option label formats, as rendered by PortConnectionModal
    // -------------------------------------------------

    public static string GroupLabel(string type, string speed, int count)
        => $"{type} — {speed} Gbps ({count})";

    public static string PortLabel(int oneBasedIndex)
        => $"Port {oneBasedIndex}";
}
