using Microsoft.Extensions.FileProviders;
using Shared.Rcl.Docs;

namespace RackPeek.Web;

/// <summary>
///     Reads the docs directly from the composed static web assets
///     (Shared.Rcl's wwwroot in development, wwwroot/_content/Shared.Rcl in a
///     published app), so the server never has to call back to its own public
///     URL — which is unreachable from inside the container behind a reverse
///     proxy (issue #304).
/// </summary>
public class StaticWebAssetDocsContentProvider(IWebHostEnvironment environment) : IDocsContentProvider {
    public async Task<string> GetAsync(string fileName) {
        IFileInfo file = environment.WebRootFileProvider
            .GetFileInfo($"_content/Shared.Rcl/raw_docs/{fileName}");

        if (!file.Exists)
            throw new FileNotFoundException($"Docs file '{fileName}' not found.");

        await using Stream stream = file.CreateReadStream();
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }
}
