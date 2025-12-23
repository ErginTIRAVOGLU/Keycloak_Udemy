using System;
using System.Text.Json.Serialization;

namespace Keycloak.WebAPI.Dto;

public sealed class GetAccessResponseDto
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = default!;
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
    [JsonPropertyName("refresh_expires_in")]
    public int RefreshExpiresIn { get; set; }
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = default!;
    [JsonPropertyName("not-before-policy")]
    public int NotBeforePolicy { get; set; } = default!;
    [JsonPropertyName("scope")]
    public string Scope { get; set; } = default!;
}
