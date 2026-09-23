using Microsoft.Extensions.DependencyInjection;
using RackPeek.Domain.Resources.OtherHardware;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Shared.Rcl.Commands.OtherHardware;

public class OtherSetSettings : OtherNameSettings {
    [CommandOption("--model")] public string? Model { get; set; }
    [CommandOption("--description")] public string? Description { get; set; }
}

public class OtherSetCommand(IServiceProvider provider)
    : AsyncCommand<OtherSetSettings> {
    protected override async Task<int> ExecuteAsync(
        CommandContext context,
        OtherSetSettings settings,
        CancellationToken cancellationToken) {
        using IServiceScope scope = provider.CreateScope();
        UpdateOtherUseCase useCase = scope.ServiceProvider.GetRequiredService<UpdateOtherUseCase>();

        await useCase.ExecuteAsync(settings.Name, settings.Model, settings.Description);

        AnsiConsole.MarkupLine($"[green]Other hardware '{settings.Name}' updated.[/]");
        return 0;
    }
}
