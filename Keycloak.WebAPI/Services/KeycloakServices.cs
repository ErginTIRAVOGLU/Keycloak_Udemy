using System;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Keycloak.WebAPI.Dto;
using Keycloak.WebAPI.Options;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Options;
using TS.Result;

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

    public async Task<object> LoginUserAsync(LoginDto request, CancellationToken cancellationToken = default)
    {
        string token = await GetAccessTokenAsync(cancellationToken);

        using var client = httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(options.Value.HostName);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var endpoint = $"/realms/{options.Value.Realm}/protocol/openid-connect/token";

        List<KeyValuePair<string, string>> parameters = new()
        {
            new KeyValuePair<string, string>("client_id", options.Value.ClientId),
            new KeyValuePair<string, string>("client_secret", options.Value.ClientSecret),
            new KeyValuePair<string, string>("username", request.Username),
            new KeyValuePair<string, string>("password", request.Password),
            new KeyValuePair<string, string>("grant_type", "password")
        };

        var parametersContent = new FormUrlEncodedContent(parameters);

        var response = await client.PostAsync(endpoint, parametersContent, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            HandleErrorResponse(response.StatusCode, content);
        }
        var result = JsonSerializer.Deserialize<object>(content);
        return result!;
    }

    public async Task<Result<IEnumerable<UserDto>>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            string token = await GetAccessTokenAsync(cancellationToken);

            using var client = httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(options.Value.HostName);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var endpoint = $"/admin/realms/{options.Value.Realm}/users";

            var response = await client.GetAsync(endpoint, cancellationToken);

            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                HandleErrorResponse(response.StatusCode, content);
            }

            var users = JsonSerializer.Deserialize<List<UserDto>>(content);
            return Result<IEnumerable<UserDto>>.Succeed(users!);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<UserDto>>.Failure(ex.Message);
        }
    }
    public async Task<Result<UserDto>> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            string token = await GetAccessTokenAsync(cancellationToken);

            using var client = httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(options.Value.HostName);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var endpoint = $"/admin/realms/{options.Value.Realm}/users?email={WebUtility.UrlEncode(email)}";

            var response = await client.GetAsync(endpoint, cancellationToken);

            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                HandleErrorResponse(response.StatusCode, content);
            }

            var users = JsonSerializer.Deserialize<List<UserDto>>(content);
            var user = users!.FirstOrDefault();
            if (user == null)
            {
                return Result<UserDto>.Failure($"User with email '{email}' not found.");
            }
            return Result<UserDto>.Succeed(user);
        }
        catch (Exception ex)
        {
            return Result<UserDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<UserDto>> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        try
        {
            string token = await GetAccessTokenAsync(cancellationToken);

            using var client = httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(options.Value.HostName);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var endpoint = $"/admin/realms/{options.Value.Realm}/users?username={WebUtility.UrlEncode(username)}";

            var response = await client.GetAsync(endpoint, cancellationToken);

            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                HandleErrorResponse(response.StatusCode, content);
            }

            var users = JsonSerializer.Deserialize<List<UserDto>>(content);
            var user = users!.FirstOrDefault();
            if (user == null)
            {
                return Result<UserDto>.Failure($"User with username '{username}' not found.");
            }
            return Result<UserDto>.Succeed(user);
        }
        catch (Exception ex)
        {
            return Result<UserDto>.Failure(ex.Message);
        }
    }
    
    public async Task<Result<UserDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            string token = await GetAccessTokenAsync(cancellationToken);

            using var client = httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(options.Value.HostName);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var endpoint = $"/admin/realms/{options.Value.Realm}/users/{id}";

            var response = await client.GetAsync(endpoint, cancellationToken);

            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                HandleErrorResponse(response.StatusCode, content);
            }

            var user = JsonSerializer.Deserialize<UserDto>(content);
            if (user == null)
            {
                return Result<UserDto>.Failure($"User with ID '{id}' not found.");
            }
            return Result<UserDto>.Succeed(user);
        }
        catch (Exception ex)
        {
            return Result<UserDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<string>> UpdateUserAsync(Guid id, UpdateUserDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            object data = new
            {
                firstName = request.FirstName,
                lastName = request.LastName,
                email = request.Email,
                enabled = request.Enabled,
                emailVerified = request.EmailVerified
            };

            string token = await GetAccessTokenAsync(cancellationToken);

            using var client = httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(options.Value.HostName);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var endpoint = $"/admin/realms/{options.Value.Realm}/users/{id}";

            string jsonData = JsonSerializer.Serialize(data);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            var response = await client.PutAsync(endpoint, content, cancellationToken);

            var message = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                HandleErrorResponse(response.StatusCode, message);
            }
            return Result<string>.Succeed("User updated successfully."); 
        }
        catch (Exception ex)
        {
            return Result<string>.Failure($"Failed to update user: {ex.Message}");
        }
    }

    public async Task<Result<string>> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            string token = await GetAccessTokenAsync(cancellationToken);

            using var client = httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(options.Value.HostName);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var endpoint = $"/admin/realms/{options.Value.Realm}/users/{id}";

            var response = await client.DeleteAsync(endpoint, cancellationToken);

            var message = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                HandleErrorResponse(response.StatusCode, message);
            }
            return Result<string>.Succeed("User deleted successfully.");
        }
        catch (Exception ex)
        {
            return Result<string>.Failure($"Failed to delete user: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<RoleDto>>> GetAllRolesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            string token = await GetAccessTokenAsync(cancellationToken);

            using var client = httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(options.Value.HostName);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var endpoint = $"/admin/realms/{options.Value.Realm}/clients/{options.Value.ClientUUID}/roles";

            var response = await client.GetAsync(endpoint, cancellationToken);

            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                HandleErrorResponse(response.StatusCode, content);
            }

            var roles = JsonSerializer.Deserialize<List<RoleDto>>(content);
            return Result<IEnumerable<RoleDto>>.Succeed(roles!);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<RoleDto>>.Failure(ex.Message);
        }
    }

    public async Task<Result<RoleDto>> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken = default)
    {
        try
        {
            string token = await GetAccessTokenAsync(cancellationToken);

            using var client = httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(options.Value.HostName);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var endpoint = $"/admin/realms/{options.Value.Realm}/clients/{options.Value.ClientUUID}/roles/{WebUtility.UrlEncode(roleName)}";

            var response = await client.GetAsync(endpoint, cancellationToken);

            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                HandleErrorResponse(response.StatusCode, content);
            }

            var role = JsonSerializer.Deserialize<RoleDto>(content);
            if (role == null)
            {
                return Result<RoleDto>.Failure($"Role with name '{roleName}' not found.");
            }
            return Result<RoleDto>.Succeed(role);
        }
        catch (Exception ex)
        {
            return Result<RoleDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<string>> CreateRoleAsync(CreateRoleDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            object data = new
            {
                name = request.Name
            };

            string token = await GetAccessTokenAsync(cancellationToken);

            using var client = httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(options.Value.HostName);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var endpoint = $"/admin/realms/{options.Value.Realm}/clients/{options.Value.ClientUUID}/roles";

            string jsonData = JsonSerializer.Serialize(data);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(endpoint, content, cancellationToken);

            var message = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                HandleErrorResponse(response.StatusCode, message);
            }

            return Result<string>.Succeed("Role created successfully.");
        }
        catch (Exception ex)
        {
            return Result<string>.Failure($"Failed to create role: {ex.Message}");
        }
    }

    public async Task<Result<string>> DeleteRoleByNameAsync(string roleName, CancellationToken cancellationToken = default)
    {
        try
        {
            string token = await GetAccessTokenAsync(cancellationToken);

            using var client = httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(options.Value.HostName);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var endpoint = $"/admin/realms/{options.Value.Realm}/clients/{options.Value.ClientUUID}/roles/{WebUtility.UrlEncode(roleName)}";

            var response = await client.DeleteAsync(endpoint, cancellationToken);

            var message = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                HandleErrorResponse(response.StatusCode, message);
            }

            return Result<string>.Succeed("Role deleted successfully.");
        }
        catch (Exception ex)
        {
            return Result<string>.Failure($"Failed to delete role: {ex.Message}");
        }
    }

    public async Task<Result<string>> AssignRolesToUserAsync(Guid userId, List<RoleDto> roles, CancellationToken cancellationToken = default)
    {
        try
        {
            string token = await GetAccessTokenAsync(cancellationToken);

            using var client = httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(options.Value.HostName);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var endpoint = $"/admin/realms/{options.Value.Realm}/users/{userId}/role-mappings/clients/{options.Value.ClientUUID}";

            string jsonData = JsonSerializer.Serialize(roles);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(endpoint, content, cancellationToken);

            var message = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                HandleErrorResponse(response.StatusCode, message);
            }

            return Result<string>.Succeed("Roles assigned to user successfully.");
        }
        catch (Exception ex)
        {
            return Result<string>.Failure($"Failed to assign roles to user: {ex.Message}");
        }
    }

    public async Task<Result<string>> UnAssignRolesToUserAsync(Guid userId, List<RoleDto> roles, CancellationToken cancellationToken = default)
    {
        try
        {
            string token = await GetAccessTokenAsync(cancellationToken);

            using var client = httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(options.Value.HostName);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var endpoint = $"/admin/realms/{options.Value.Realm}/users/{userId}/role-mappings/clients/{options.Value.ClientUUID}";

            string jsonData = JsonSerializer.Serialize(roles);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Delete, endpoint)
            {
                Content = content
            };

            var response = await client.SendAsync(request, cancellationToken);

            var message = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                HandleErrorResponse(response.StatusCode, message);
            }

            return Result<string>.Succeed("Roles unassigned from user successfully.");
        }
        catch (Exception ex)
        {
            return Result<string>.Failure($"Failed to unassign roles from user: {ex.Message}");
        }
    }

    public async Task<Result<List<RoleDto>>> GetAllUserRoles(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            string token = await GetAccessTokenAsync(cancellationToken);

            using var client = httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(options.Value.HostName);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var endpoint = $"/admin/realms/{options.Value.Realm}/users/{userId}/role-mappings/clients/{options.Value.ClientUUID}";

            var response = await client.GetAsync(endpoint, cancellationToken);

            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                HandleErrorResponse(response.StatusCode, content);
            }

            var roles = JsonSerializer.Deserialize<List<RoleDto>>(content);
            return Result<List<RoleDto>>.Succeed(roles!);
        }
        catch (Exception ex)
        {
            return Result<List<RoleDto>>.Failure(ex.Message);
        }
    }


    private void HandleErrorResponse(HttpStatusCode statusCode, string content)
    {
        switch (statusCode)
        {
            case HttpStatusCode.NotFound:
                {
                    var errorResult = JsonSerializer.Deserialize<ErrorResponseDto>(content);
                    throw new InvalidOperationException($"The Keycloak server, not found error. Error: {errorResult!.Error}");
                }
            case HttpStatusCode.Unauthorized:
                {
                    var errorResult = JsonSerializer.Deserialize<ErrorResponseDto>(content);
                    throw new InvalidOperationException($"Unauthorized access to Keycloak server. Error: {errorResult!.ErrorDescription}");
                }
            case HttpStatusCode.BadRequest:
                {
                    var errorResult = JsonSerializer.Deserialize<ErrorResponseDto>(content);
                    throw new InvalidOperationException($"Bad request to Keycloak server: {errorResult!.ErrorDescription}");
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
