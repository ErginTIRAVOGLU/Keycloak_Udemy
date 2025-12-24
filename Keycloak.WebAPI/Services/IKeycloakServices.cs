using Keycloak.WebAPI.Dto;

namespace Keycloak.WebAPI.Services;

public interface IKeycloakServices
{
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
    Task RegisterUserAsync(RegisterDto request, CancellationToken cancellationToken = default);
}