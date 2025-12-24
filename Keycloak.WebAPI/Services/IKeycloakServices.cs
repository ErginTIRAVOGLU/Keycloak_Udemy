namespace Keycloak.WebAPI.Services;

public interface IKeycloakServices
{
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}