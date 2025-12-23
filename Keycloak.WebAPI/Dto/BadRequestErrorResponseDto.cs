using System.Text.Json.Serialization;

namespace Keycloak.WebAPI.Dto;

public sealed class BadRequestErrorResponseDto
{
    [JsonPropertyName("error")]
    public string Field { get; set; } = default!;
    [JsonPropertyName("errorMessage")]
    public string ErrorMessage { get; set; } = default!;
}
