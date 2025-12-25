using System.Text.Json.Serialization;

namespace Keycloak.WebAPI.Dto
{
    public sealed record UserDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }
        [JsonPropertyName("username")]
        public string Username { get; set; } = default!;
        [JsonPropertyName("firstName")]
        public string FirstName { get; set; } = default!;
        [JsonPropertyName("lastName")]
        public string LastName { get; set; } = default!;
        [JsonPropertyName("email")]
        public string Email { get; set; } = default!;
        [JsonPropertyName("emailVerified")]
        public bool EmailVerified { get; set; } = default!;
        [JsonPropertyName("createdTimestamp")]
        public long CreatedTimestamp { get; set; }
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }
    }
}
