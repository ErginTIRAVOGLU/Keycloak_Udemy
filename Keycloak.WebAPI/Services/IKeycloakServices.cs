using Keycloak.WebAPI.Dto;
using TS.Result;

namespace Keycloak.WebAPI.Services;

public interface IKeycloakServices
{
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);

    Task RegisterUserAsync(RegisterDto request, CancellationToken cancellationToken = default);
    Task<object> LoginUserAsync(LoginDto request, CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<UserDto>>> GetAllUsersAsync(CancellationToken cancellationToken = default);
    Task<Result<UserDto>> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Result<UserDto>> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<Result<UserDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<string>> UpdateUserAsync(Guid id, UpdateUserDto request, CancellationToken cancellationToken = default);
    Task<Result<string>> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<RoleDto>>> GetAllRolesAsync(CancellationToken cancellationToken = default);
    Task<Result<RoleDto>> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken = default);
    Task<Result<string>> CreateRoleAsync(CreateRoleDto request, CancellationToken cancellationToken = default);
    Task<Result<string>> DeleteRoleByNameAsync(string roleName, CancellationToken cancellationToken = default);

    Task<Result<string>> AssignRolesToUserAsync(Guid userId, List<RoleDto> roles, CancellationToken cancellationToken = default);
    Task<Result<string>> UnAssignRolesToUserAsync(Guid userId, List<RoleDto> roles, CancellationToken cancellationToken = default);
    Task<Result<List<RoleDto>>> GetAllUserRoles(Guid userId, CancellationToken cancellationToken = default);

}