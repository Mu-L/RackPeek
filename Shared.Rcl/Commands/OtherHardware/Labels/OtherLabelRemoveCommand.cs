using Microsoft.Extensions.DependencyInjection;
using RackPeek.Domain.UseCases.Labels;
using Spectre.Console;
using Spectre.Console.Cli;
using OtherResource = RackPeek.Domain.Resources.OtherHardware.Other;

namespace Shared.Rcl.Commands.OtherHardware.Labels;

public class OtherLabelRemoveSettings : OtherNameSettings {
    [CommandOption("--key <KEY>")] public string Key { get; set; } = default!;
}

public class OtherLabelRemoveCommand(IServiceProvider serviceProvider) : AsyncCommand<OtherLabelRemoveSettings> {
    protected override async Task<int> ExecuteAsync(CommandContext context, OtherLabelRemoveSettings settings,
        CancellationToken cancellationToken) {
        using IServiceScope scope = serviceProvider.CreateScope();
        IRemoveLabelUseCase<OtherResource> useCase = scope.ServiceProvider.GetRequiredService<IRemoveLabelUseCase<OtherResource>>();
        await useCase.ExecuteAsync(settings.Name, settings.Key);
        AnsiConsole.MarkupLine($"[green]Label '{settings.Key}' removed from '{settings.Name}'.[/]");
        return 0;
    }
}
