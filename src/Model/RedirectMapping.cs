using System.Text.Json.Serialization;

namespace ExeRedirector.Model;

internal sealed record RedirectMapping
{
    [JsonPropertyName("path")]
    public string? Path { get; init; }

    [JsonPropertyName("filetype")]
    public string? FileType { get; init; }

    [JsonPropertyName("app")]
    public string? App { get; init; }

    [JsonPropertyName("arguments")]
    public string[]? Arguments { get; init; }
}
