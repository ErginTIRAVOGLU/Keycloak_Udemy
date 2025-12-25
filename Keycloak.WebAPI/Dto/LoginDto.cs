namespace Keycloak.WebAPI.Dto
{
    public sealed record LoginDto(
        string Username,
        string Password
    );
}
