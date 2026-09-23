using Microsoft.Extensions.DependencyInjection;
using RackPeek.Domain.UseCases;
using Spectre.Console;
using Spectre.Console.Cli;
using OtherResource = RackPeek.Domain.Resources.OtherHardware.Other;

namespace Shared.Rcl.Commands.OtherHardware;

public class OtherAddSettings : CommandSettings {
    [CommandArgument(0, "<name>")] public string Name { get; set; } = default!;
}

public class OtherAddCommand(IServiceProvider provider)
    : AsyncCommand<OtherAddSettings> {
    protected override async Task<int> ExecuteAsync(
        CommandContext context,
        OtherAddSettings settings,
        CancellationToken cancellationToken) {
        using IServiceScope scope = provider.CreateScope();
        IAddResourceUseCase<OtherResource> useCase = scope.ServiceProvider
            .GetRequiredService<IAddResourceUseCase<OtherResource>>();

        await useCase.ExecuteAsync(settings.Name);

        AnsiConsole.MarkupLine($"[green]Other hardware '{settings.Name}' added.[/]");
        return 0;
    }
}
