using System.Text.Json.Serialization;

namespace Keycloak.WebAPI.Dto
{
    public sealed record UpdateUserDto
    {
       
        [JsonPropertyName("firstName")]
        public string? FirstName { get; set; }
        [JsonPropertyName("lastName")]
        public string? LastName { get; set; }
        [JsonPropertyName("email")]
        public string? Email { get; set; }
         [JsonPropertyName("emailVerified")]
        public bool? EmailVerified { get; set; }
        [JsonPropertyName("enabled")]
        public bool? Enabled { get; set; }
    }
}
