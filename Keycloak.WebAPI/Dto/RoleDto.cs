using System.Text.Json.Serialization;

namespace Keycloak.WebAPI.Dto
{
    public sealed record RoleDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }= default!;
        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }
}
