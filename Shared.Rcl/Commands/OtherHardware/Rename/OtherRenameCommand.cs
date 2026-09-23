using Microsoft.Extensions.DependencyInjection;
using RackPeek.Domain.UseCases;
using Spectre.Console;
using Spectre.Console.Cli;
using OtherResource = RackPeek.Domain.Resources.OtherHardware.Other;

namespace Shared.Rcl.Commands.OtherHardware.Rename;

public class OtherRenameSettings : OtherNameSettings {
    [CommandArgument(1, "<new-name>")]
    public string NewName { get; set; } = default!;
}

public class OtherRenameCommand(
    IServiceProvider serviceProvider
) : AsyncCommand<OtherRenameSettings> {
    protected override async Task<int> ExecuteAsync(
        CommandContext context,
        OtherRenameSettings settings,
        CancellationToken cancellationToken) {
        using IServiceScope scope = serviceProvider.CreateScope();
        IRenameResourceUseCase<OtherResource> renameUseCase = scope.ServiceProvider.GetRequiredService<IRenameResourceUseCase<OtherResource>>();

        await renameUseCase.ExecuteAsync(settings.Name, settings.NewName);

        AnsiConsole.MarkupLine($"[green]Other hardware '{settings.Name}' renamed to '{settings.NewName}'.[/]");
        return 0;
    }
}
