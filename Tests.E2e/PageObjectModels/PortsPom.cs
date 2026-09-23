namespace Tests.E2e.PageObjectModels;

using Microsoft.Playwright;

public class PortsPom(IPage page) {
    public TagsPom Tags => new(page);
    public LabelsPom Labels => new(page);
    public PortsPom Ports => new(page);

    private const string _portsPrefix = "accesspoint-ports";

    // -------------------------------------------------
    // Root
    // -------------------------------------------------

    public ILocator Root(string testIdPrefix)
        => page.GetByTestId($"{testIdPrefix}-port-group-section");

    public ILocator AddButton(string testIdPrefix)
        => page.GetByTestId($"{testIdPrefix}-port-group-add");

    // -------------------------------------------------
    // Port Groups
    // -------------------------------------------------

    public ILocator PortGroup(string testIdPrefix, int index)
        => page.GetByTestId($"{testIdPrefix}-port-group-item-{index}");

    public ILocator EditPortGroupButton(string testIdPrefix, int index)
        => page.GetByTestId($"{testIdPrefix}-port-group-edit-{index}");

    public ILocator PortsContainer(string testIdPrefix, int index)
        => page.GetByTestId($"{testIdPrefix}-port-group-ports-{index}");

    // -------------------------------------------------
    // Individual Ports
    // -------------------------------------------------

    public ILocator Port(string testIdPrefix, int groupIndex, int portIndex)
        => page.GetByTestId($"{testIdPrefix}-port-group-visualizer-{groupIndex}-port-{portIndex}");

    // -------------------------------------------------
    // Port Modal
    // -------------------------------------------------

    public ILocator PortModal(string testIdPrefix)
        => page.GetByTestId($"{testIdPrefix}-port-group-port-modal");

    // -------------------------------------------------
    // Connection Modal
    // -------------------------------------------------

    /// <summary>
    ///     A resource card mounts PortConnectionModal under its ports prefix,
    ///     so the modal's own test ids are built from "{prefix}-port-group".
    /// </summary>
    public ConnectionModalPom ConnectionModalFor(string testIdPrefix)
        => new(page, $"{testIdPrefix}-port-group");

    public ILocator ConnectionModal(string testIdPrefix)
        => ConnectionModalFor(testIdPrefix).Container;

    public ILocator ResourceASelect(string testIdPrefix)
        => ConnectionModalFor(testIdPrefix).ResourceASelect;

    public ILocator GroupASelect(string testIdPrefix)
        => ConnectionModalFor(testIdPrefix).GroupASelect;

    public ILocator PortASelect(string testIdPrefix)
        => ConnectionModalFor(testIdPrefix).PortASelect;

    public ILocator ResourceBSelect(string testIdPrefix)
        => ConnectionModalFor(testIdPrefix).ResourceBSelect;

    public ILocator GroupBSelect(string testIdPrefix)
        => ConnectionModalFor(testIdPrefix).GroupBSelect;

    public ILocator PortBSelect(string testIdPrefix)
        => ConnectionModalFor(testIdPrefix).PortBSelect;

    public ILocator SubmitConnection(string testIdPrefix)
        => ConnectionModalFor(testIdPrefix).SubmitButton;

    public ILocator LabelInput(string testIdPrefix)
        => ConnectionModalFor(testIdPrefix).LabelInput;

    // -------------------------------------------------
    // Assertions
    // -------------------------------------------------

    public async Task AssertPortGroupVisibleAsync(string prefix, int index)
        => await Assertions.Expect(PortGroup(prefix, index)).ToBeVisibleAsync();

    public async Task AssertPortVisibleAsync(string prefix, int groupIndex, int portIndex)
        => await Assertions.Expect(Port(prefix, groupIndex, portIndex)).ToBeVisibleAsync();

    // -------------------------------------------------
    // Actions
    // -------------------------------------------------

    public async Task AddPortGroupAsync(string prefix) {
        await AddButton(prefix).ClickAsync();
        await Assertions.Expect(PortModal(prefix)).ToBeVisibleAsync();
    }

    public async Task OpenConnectionFromPortAsync(string prefix, int groupIndex, int portIndex) {
        await Port(prefix, groupIndex, portIndex).ClickAsync();
        await Assertions.Expect(ConnectionModal(prefix)).ToBeVisibleAsync();
    }

    public async Task CreateConnectionAsync(
        string prefix,
        string resourceA,
        string groupA,
        string portA,
        string resourceB,
        string groupB,
        string portB,
        string? label = null)
        => await ConnectionModalFor(prefix).CreateConnectionAsync(
            resourceA, groupA, portA, resourceB, groupB, portB, label);

    // -------------------------------------------------
    // Port Modal Fields
    // -------------------------------------------------

    public ILocator PortTypeSelect(string prefix)
        => page.GetByTestId($"{prefix}-port-group-port-modal-type-input");

    public ILocator PortSpeedSelect(string prefix)
        => page.GetByTestId($"{prefix}-port-group-port-modal-speed-input");

    public ILocator PortCountInput(string prefix)
        => page.GetByTestId($"{prefix}-port-group-port-modal-count-input");
    public ILocator PortSubmit(string prefix)
        => page.GetByTestId($"{prefix}-port-group-port-modal-submit");

    public ILocator PortCancel(string prefix)
        => page.GetByTestId($"{prefix}-port-group-port-modal-cancel");

    public async Task AddPortGroupAsync(
        string prefix,
        string type,
        string speed,
        int count) {
        await AddButton(prefix).ClickAsync();

        await Assertions.Expect(PortModal(prefix)).ToBeVisibleAsync();

        await PortTypeSelect(prefix).SelectOptionAsync(
            new SelectOptionValue { Label = type });

        await PortSpeedSelect(prefix).FillAsync(speed.ToString());

        await PortCountInput(prefix).FillAsync(count.ToString());

        await PortSubmit(prefix).ClickAsync();
    }
}
