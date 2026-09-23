using Microsoft.Extensions.DependencyInjection;
using RackPeek.Domain.UseCases;
using Spectre.Console;
using Spectre.Console.Cli;
using OtherResource = RackPeek.Domain.Resources.OtherHardware.Other;

namespace Shared.Rcl.Commands.OtherHardware;

public class OtherNameSettings : CommandSettings {
    [CommandArgument(0, "<name>")] public string Name { get; set; } = default!;
}

public class OtherDeleteCommand(IServiceProvider provider)
    : AsyncCommand<OtherNameSettings> {
    protected override async Task<int> ExecuteAsync(
        CommandContext context,
        OtherNameSettings settings,
        CancellationToken cancellationToken) {
        using IServiceScope scope = provider.CreateScope();
        IDeleteResourceUseCase<OtherResource> useCase = scope.ServiceProvider
            .GetRequiredService<IDeleteResourceUseCase<OtherResource>>();

        await useCase.ExecuteAsync(settings.Name);

        AnsiConsole.MarkupLine($"[green]Other hardware '{settings.Name}' deleted.[/]");
        return 0;
    }
}
