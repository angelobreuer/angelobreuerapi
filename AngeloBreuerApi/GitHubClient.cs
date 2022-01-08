namespace AngeloBreuerApi;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;

internal sealed class GitHubClient
{
    private static readonly string UserAgent = $".NET HTTP Client/{Environment.Version}";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptions<AppConfiguration> _options;

    public GitHubClient(IHttpClientFactory httpClientFactory, IOptions<AppConfiguration> options)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public async ValueTask<ProjectProperties> FetchAsync(string projectName, CancellationToken cancellationToken = default)
    {
        using var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);

        var uri = new Uri($"https://api.github.com/repos/{projectName}");

        using var document = await httpClient
            .GetFromJsonAsync<JsonDocument>(uri, cancellationToken)
            .ConfigureAwait(false);

        var stargazers = document!.RootElement.GetProperty("stargazers_count").GetInt32();
        var forks = document!.RootElement.GetProperty("forks_count").GetInt32();
        var language = document!.RootElement.GetProperty("language").GetString()!;

        if (!_options.Value.LanguageColorMap.TryGetValue(language, out var languageColor))
        {
            languageColor = "#131313";
        }

        var languageProperties = new LanguageProperties
        {
            Color = languageColor,
            Name = language,
        };

        var projectProperties = new ProjectProperties
        {
            Forks = forks,
            Language = languageProperties,
            Stargazers = stargazers,
        };

        return projectProperties;
    }
}
