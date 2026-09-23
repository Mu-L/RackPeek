using Microsoft.Extensions.DependencyInjection;
using RackPeek.Domain.Resources.OtherHardware;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Shared.Rcl.Commands.OtherHardware;

public class OtherGetByNameCommand(IServiceProvider provider)
    : AsyncCommand<OtherNameSettings> {
    protected override async Task<int> ExecuteAsync(
        CommandContext context,
        OtherNameSettings settings,
        CancellationToken cancellationToken) {
        using IServiceScope scope = provider.CreateScope();
        DescribeOtherUseCase useCase = scope.ServiceProvider.GetRequiredService<DescribeOtherUseCase>();

        OtherDescription other = await useCase.ExecuteAsync(settings.Name);

        AnsiConsole.MarkupLine(
            $"[green]{other.Name}[/]  Model: {other.Model ?? "Unknown"}, Description: {other.Description ?? "Unknown"}");

        return 0;
    }
}
