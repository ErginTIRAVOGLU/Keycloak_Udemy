using Keycloak.WebAPI.Dto;
using Keycloak.WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Keycloak.WebAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class UserRolesController(
        IKeycloakServices keycloakServices
    ) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> AssignRolesToUser(Guid userId, List<RoleDto> roles, CancellationToken cancellationToken)
        {
            var result = await keycloakServices.AssignRolesToUserAsync(userId, roles, cancellationToken);
            if (result.IsSuccessful)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpDelete]
        public async Task<IActionResult> UnAssignRolesToUser(Guid userId, List<RoleDto> roles, CancellationToken cancellationToken)
        {
            var result = await keycloakServices.UnAssignRolesToUserAsync(userId, roles, cancellationToken);
            if (result.IsSuccessful)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUserRoles(Guid userId, CancellationToken cancellationToken)
        {
            var result = await keycloakServices.GetAllUserRoles(userId, cancellationToken);
            if (result.IsSuccessful)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}
