using System;
using System.Text.Json.Serialization;

namespace Keycloak.WebAPI.Dto;

public sealed class ErrorResponseDto
{
    [JsonPropertyName("error")]
    public string Error { get; set; } = default!;
    [JsonPropertyName("error_description")]
    public string ErrorDescription { get; set; } = default!;
}
