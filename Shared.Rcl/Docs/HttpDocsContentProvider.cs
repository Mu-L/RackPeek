namespace Shared.Rcl.Docs;

public class HttpDocsContentProvider(HttpClient http) : IDocsContentProvider {
    public Task<string> GetAsync(string fileName) =>
        http.GetStringAsync($"_content/Shared.Rcl/raw_docs/{fileName}");
}
