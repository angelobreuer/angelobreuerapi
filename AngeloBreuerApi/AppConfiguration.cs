namespace AngeloBreuerApi;

public sealed class AppConfiguration
{
    public IReadOnlyList<string> Projects { get; set; } = null!;

    public IDictionary<string, string> LanguageColorMap { get; set; } = null!;
}
