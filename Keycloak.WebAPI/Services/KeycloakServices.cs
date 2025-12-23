using System;
using System.Net;
using System.Text.Json;
using Keycloak.WebAPI.Dto;
using Keycloak.WebAPI.Options;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Options;

namespace Keycloak.WebAPI.Services;

public sealed class KeycloakServices(
    IOptions<KeycloakConfiguration> options,
    IHttpClientFactory httpClientFactory) : IKeycloakServices
{
    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        using var client = httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(options.Value.HostName);
        var endpoint = $"/realms/{options.Value.Realm}/protocol/openid-connect/token";

        List<KeyValuePair<string, string>> parameters = new()
        {
            new KeyValuePair<string, string>("client_id", options.Value.ClientId),
            new KeyValuePair<string, string>("client_secret", options.Value.ClientSecret),
            new KeyValuePair<string, string>("grant_type", "client_credentials")
        };

        var response = await client.PostAsync(
            endpoint, new FormUrlEncodedContent(parameters), cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if(!response.IsSuccessStatusCode)
        {
            HandleErrorResponse(response.StatusCode, content);
        }

        var result  = JsonSerializer.Deserialize<GetAccessResponseDto>(content);
        return result!.AccessToken;
    }

    private void HandleErrorResponse(HttpStatusCode statusCode, string content)
    {
        switch (statusCode)
        {
            case HttpStatusCode.NotFound:
                {
                    var errorResult = JsonSerializer.Deserialize<ErrorResponseDto>(content);
                    throw new InvalidOperationException($"The Keycloak realm '{options.Value.Realm}' or client '{options.Value.ClientId}' was not found at '{options.Value.HostName}'. Error: {errorResult!.ErrorDescription}");
                }
            case HttpStatusCode.Unauthorized:
                {
                    var errorResult = JsonSerializer.Deserialize<ErrorResponseDto>(content);
                    throw new InvalidOperationException($"Unauthorized access to Keycloak server. Error: {errorResult!.ErrorDescription}");
                }
            case HttpStatusCode.BadRequest:
                {
                    var errorResult = JsonSerializer.Deserialize<BadRequestErrorResponseDto>(content);
                    throw new InvalidOperationException($"Bad request to Keycloak server: {errorResult!.ErrorMessage}");
                }
            default:
                {
                    var errorResult = JsonSerializer.Deserialize<ErrorResponseDto>(content);
                    throw new InvalidOperationException($"An error occurred while requesting access token from Keycloak server. Status Code: {statusCode}, Error: {errorResult!.ErrorDescription}");
                }
        }
    }

}

 
public interface IKeycloakServices
{
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}