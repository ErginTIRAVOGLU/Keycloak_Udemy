using System.Text.Json.Serialization;

namespace Keycloak.WebAPI.Dto
{
    public sealed record CreateRoleDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }= default!;
        [JsonPropertyName("description")]
        public string? Description { get; set; }= default!;
    }
}
