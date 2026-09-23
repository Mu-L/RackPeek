using RackPeek.Domain.Resources.SystemResources;

namespace Shared.Rcl.Systems;

public sealed class SystemEditModel {
    private string? _type;
    public string Name { get; init; } = default!;

    public string? Type {
        get => _type;
        set => _type = string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim().ToLowerInvariant();
    }

    public string? Ip { get; set; }
    public string? Os { get; set; }
    public int? Cores { get; set; }
    public double? Ram { get; set; }
    public List<string> RunsOn { get; set; } = new();
    public string? Notes { get; set; }

    public static SystemEditModel From(SystemResource system) {
        return new SystemEditModel {
            Name = system.Name,
            // The type dropdown has no empty option, so a system without a
            // type still displays the first choice; default to it so what the
            // user sees is what gets saved (#306).
            Type = system.Type ?? SystemResource.ValidSystemTypes[0],
            Os = system.Os,
            Cores = system.Cores,
            Ram = system.Ram,
            Ip = system.Ip,
            RunsOn = system.RunsOn,
            Notes = system.Notes
        };
    }
}
