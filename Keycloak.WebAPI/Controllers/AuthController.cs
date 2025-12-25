using Keycloak.WebAPI.Dto;
using Keycloak.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using TS.Result;

namespace Keycloak.WebAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController(
        IKeycloakServices keycloakServices
    ) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto request, CancellationToken cancellationToken)
        {
            try
            {
                await keycloakServices.RegisterUserAsync(request, cancellationToken);
                return Ok(Result<string>.Succeed("User registered successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(Result<string>.Failure(ex.Message));
            }
        }

         [HttpPost]
        public async Task<IActionResult> Login(LoginDto request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await keycloakServices.LoginUserAsync(request, cancellationToken);
                return Ok(Result<object>.Succeed(result));
            }
            catch (Exception ex)
            {
                return BadRequest(Result<string>.Failure(ex.Message));
            }
        }
    }
}
