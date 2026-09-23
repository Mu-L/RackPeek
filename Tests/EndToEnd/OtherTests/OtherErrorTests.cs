using Tests.EndToEnd.Infra;
using Xunit.Abstractions;

namespace Tests.EndToEnd.OtherTests;

[Collection("Yaml CLI tests")]
public class OtherErrorTests(TempYamlCliFixture fs, ITestOutputHelper outputHelper)
    : IClassFixture<TempYamlCliFixture> {
    private async Task<(string, string)> ExecuteAsync(params string[] args) {
        var output = await YamlCliTestHost.RunAsync(
            args,
            fs.Root,
            outputHelper,
            "config.yaml");

        var yaml = await File.ReadAllTextAsync(Path.Combine(fs.Root, "config.yaml"));
        return (output, yaml);
    }

    [Fact]
    public async Task adding_duplicate_other_returns_error() {
        await ExecuteAsync("other", "add", "radio01");

        (var output, var _) = await ExecuteAsync("other", "add", "radio01");

        Assert.Contains("already exists", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task get_missing_other_returns_error() {
        (var output, var _) = await ExecuteAsync("other", "get", "ghost");

        Assert.Contains("not found", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task set_missing_other_returns_error() {
        (var output, var _) = await ExecuteAsync(
            "other", "set", "ghost",
            "--model", "X",
            "--description", "Y"
        );

        Assert.Contains("not found", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task delete_missing_other_returns_error() {
        (var output, var _) = await ExecuteAsync("other", "del", "ghost");

        Assert.Contains("not found", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task rename_missing_other_returns_error() {
        (var output, var _) = await ExecuteAsync("other", "rename", "ghost", "ghost-new");

        Assert.Contains("not found", output, StringComparison.OrdinalIgnoreCase);
    }
}
