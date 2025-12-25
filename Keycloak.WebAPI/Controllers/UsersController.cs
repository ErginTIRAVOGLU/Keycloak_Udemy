using System;
using Keycloak.WebAPI.Dto;
using Keycloak.WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TS.Result;

namespace Keycloak.WebAPI.Controllers;


[Route("api/[controller]/[action]")]
[ApiController]
[Authorize(Policy = "users")]
public class UsersController(
    IKeycloakServices keycloakServices
) : ControllerBase
{

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        try
        {
            var users = await keycloakServices.GetAllUsersAsync(cancellationToken);
            return Ok(users);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetByEmail(string email, CancellationToken cancellationToken)
    {
        var result = await keycloakServices.GetByEmailAsync(email, cancellationToken);
        if (result.IsSuccessful)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }


    [HttpGet]
    public async Task<IActionResult> GetByUsername(string username, CancellationToken cancellationToken)
    {
        var result = await keycloakServices.GetByUsernameAsync(username, cancellationToken);
        if (result.IsSuccessful)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await keycloakServices.GetByIdAsync(id, cancellationToken);
        if (result.IsSuccessful)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpPut]
    [Route("{id}")]
    public async Task<IActionResult> UpdateUser([FromRoute] Guid id, [FromBody] UpdateUserDto request, CancellationToken cancellationToken)
    {
        var result = await keycloakServices.UpdateUserAsync(id, request, cancellationToken);
        if (result.IsSuccessful)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }   

    [HttpDelete]
    [Route("{id}")]
    public async Task<IActionResult> DeleteUser([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await keycloakServices.DeleteUserAsync(id, cancellationToken);
        if (result.IsSuccessful)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }
}
