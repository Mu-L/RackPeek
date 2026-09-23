using Microsoft.Extensions.DependencyInjection;
using RackPeek.Domain.UseCases.Labels;
using Spectre.Console;
using Spectre.Console.Cli;
using OtherResource = RackPeek.Domain.Resources.OtherHardware.Other;

namespace Shared.Rcl.Commands.OtherHardware.Labels;

public class OtherLabelAddSettings : OtherNameSettings {
    [CommandOption("--key <KEY>")] public string Key { get; set; } = default!;
    [CommandOption("--value <VALUE>")] public string Value { get; set; } = default!;
}

public class OtherLabelAddCommand(IServiceProvider serviceProvider) : AsyncCommand<OtherLabelAddSettings> {
    protected override async Task<int> ExecuteAsync(CommandContext context, OtherLabelAddSettings settings,
        CancellationToken cancellationToken) {
        using IServiceScope scope = serviceProvider.CreateScope();
        IAddLabelUseCase<OtherResource> useCase = scope.ServiceProvider.GetRequiredService<IAddLabelUseCase<OtherResource>>();
        await useCase.ExecuteAsync(settings.Name, settings.Key, settings.Value);
        AnsiConsole.MarkupLine($"[green]Label '{settings.Key}' added to '{settings.Name}'.[/]");
        return 0;
    }
}
