using RackPeek.Domain.Resources.Connections;

namespace RackPeek.Domain.Persistence;

public static class ConnectionMerger {
    /// <summary>
    ///     Merges an imported connections section into the existing set.
    ///     Returns null when the import carries no connections section — the
    ///     existing connections are kept untouched in that case (#308).
    ///     Replace mode swaps the whole set; Merge mode applies the same
    ///     overwrite rule as the UI (a port holds at most one connection, so
    ///     each incoming connection evicts anything touching its endpoints).
    /// </summary>
    public static List<Connection>? Merge(
        IReadOnlyList<Connection> existing,
        List<Connection>? incoming,
        MergeMode mode) {
        if (incoming == null)
            return null;

        if (mode == MergeMode.Replace)
            return incoming.ToList();

        var merged = existing.ToList();

        foreach (Connection connection in incoming) {
            merged.RemoveAll(c =>
                Touches(c, connection.A) || Touches(c, connection.B));

            merged.Add(connection);
        }

        return merged;
    }

    public static string Describe(Connection c) =>
        $"{Describe(c.A)} <-> {Describe(c.B)}";

    private static string Describe(PortReference p) =>
        $"{p.Resource}[{p.PortGroup}.{p.PortIndex}]";

    private static bool Touches(Connection c, PortReference port) =>
        PortsMatch(c.A, port) || PortsMatch(c.B, port);

    private static bool PortsMatch(PortReference a, PortReference b) {
        return a.Resource.Equals(b.Resource, StringComparison.OrdinalIgnoreCase)
               && a.PortGroup == b.PortGroup
               && a.PortIndex == b.PortIndex;
    }
}
