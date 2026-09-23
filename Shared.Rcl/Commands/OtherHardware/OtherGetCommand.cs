using Microsoft.Extensions.DependencyInjection;
using RackPeek.Domain.Resources.OtherHardware;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Shared.Rcl.Commands.OtherHardware;

public class OtherGetCommand(IServiceProvider provider)
    : AsyncCommand {
    protected override async Task<int> ExecuteAsync(
        CommandContext context,
        CancellationToken cancellationToken) {
        using IServiceScope scope = provider.CreateScope();
        OtherHardwareReportUseCase useCase = scope.ServiceProvider.GetRequiredService<OtherHardwareReportUseCase>();

        OtherHardwareReport report = await useCase.ExecuteAsync();

        if (report.Others.Count == 0) {
            AnsiConsole.MarkupLine("[yellow]No other hardware found.[/]");
            return 0;
        }

        Table table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Name")
            .AddColumn("Model")
            .AddColumn("Description");

        foreach (OtherHardwareRow other in report.Others)
            table.AddRow(
                other.Name.EscapeMarkup(),
                other.Model.EscapeMarkup(),
                other.Description.EscapeMarkup()
            );

        AnsiConsole.Write(table);
        return 0;
    }
}
