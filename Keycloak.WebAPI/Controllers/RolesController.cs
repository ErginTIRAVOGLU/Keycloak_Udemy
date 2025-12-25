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
    public class RolesController(
            IKeycloakServices keycloakServices
        ) : ControllerBase
    {
        [HttpGet]
        [Authorize(Policy = "RolesGetAll")]
        public async Task<IActionResult> GetAllRoles(CancellationToken cancellationToken)
        {

            var roles = await keycloakServices.GetAllRolesAsync(cancellationToken);
            if (roles.IsSuccessful)
            {
                return Ok(roles);
            }
            return BadRequest(roles);
        }

        [HttpGet]
        public async Task<IActionResult> GetRoleByName(string roleName, CancellationToken cancellationToken)
        {
            var result = await keycloakServices.GetRoleByNameAsync(roleName, cancellationToken);
            if (result.IsSuccessful)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost]
        [Authorize(Policy = "RolesCreate")]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto request, CancellationToken cancellationToken)
        {
            var result = await keycloakServices.CreateRoleAsync(request, cancellationToken);
            if (result.IsSuccessful)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteRoleByName(string roleName, CancellationToken cancellationToken)
        {
            var result = await keycloakServices.DeleteRoleByNameAsync(roleName, cancellationToken);
            if (result.IsSuccessful)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}
