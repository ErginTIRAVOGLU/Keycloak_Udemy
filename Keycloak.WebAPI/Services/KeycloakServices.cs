using System;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
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
        
        var parametersContent = new FormUrlEncodedContent(parameters);

        var response = await client.PostAsync(endpoint, parametersContent, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if(!response.IsSuccessStatusCode)
        {
            HandleErrorResponse(response.StatusCode, content);
        }

        var result  = JsonSerializer.Deserialize<GetAccessResponseDto>(content);
        return result!.AccessToken;
    }

    public async Task RegisterUserAsync(RegisterDto request, CancellationToken cancellationToken = default)
    {
        object data = new
        {
            username = request.Username,
            firstName = request.FirstName,
            lastName = request.LastName,
            email = request.Email,
            enabled = true,
            emailVerified = false,
            credentials = new[]
            {
                new
                {
                    type = "password",
                    value = request.Password,
                    temporary = false
                }
            }
        };

        string token = await GetAccessTokenAsync(cancellationToken);

        using var client = httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(options.Value.HostName);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        var endpoint = $"/admin/realms/{options.Value.Realm}/users";

        string jsonData = JsonSerializer.Serialize(data);
        var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(endpoint, content, cancellationToken);

        var message = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            HandleErrorResponse(response.StatusCode, message);
        }
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
            case HttpStatusCode.Conflict:
                {
                    var errorResult = JsonSerializer.Deserialize<BadRequestErrorResponseDto>(content);
                    throw new InvalidOperationException($"Conflict error from Keycloak server: {errorResult!.ErrorMessage}");
                }
            default:
                {
                    var errorResult = JsonSerializer.Deserialize<ErrorResponseDto>(content);
                    throw new InvalidOperationException($"An error occurred while requesting access token from Keycloak server. Status Code: {statusCode}, Error: {errorResult!.ErrorDescription}");
                }
        }
    }

}
