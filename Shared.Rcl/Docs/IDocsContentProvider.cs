namespace Shared.Rcl.Docs;

/// <summary>
///     Supplies the raw documentation assets shipped under
///     _content/Shared.Rcl/raw_docs. The Blazor Server host reads them from
///     the static web assets on disk; fetching them over HTTP would require
///     the container to reach its own public URL, which fails behind a
///     reverse proxy (issue #304). The WebAssembly viewer fetches them over
///     HTTP relative to the app base, which is always reachable there.
/// </summary>
public interface IDocsContentProvider {
    /// <summary>
    ///     Returns the content of a file under raw_docs (e.g. "overview.md",
    ///     "docs-index.json"). Throws if the file does not exist.
    /// </summary>
    Task<string> GetAsync(string fileName);
}
