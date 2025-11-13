using Microsoft.AspNetCore.Mvc;
using Rates.Gateway.Dtos;

namespace Rates.Gateway.Controllers;

[Route("api/v1/users")]
[ApiController]
public class UsersController(Rates.Gateway.Services.UsersService usersService) : ControllerBase
{
    [HttpPost("authenticate")]
    public async Task<AuthenticationResponseDto> Authenticate([FromBody] AuthenticateDto request)
    {
        return await usersService.Authenticate(request.Username, request.Password);
    }

    [HttpPost("register")]
    public async Task Register(RegisterDto request)
    {
        await usersService.Register(request.Username, request.Password);
    }

    [HttpPost("revoke-token")]
    public async Task RevokeToken([FromBody] string token)
    {
        await usersService.RevokeToken(token);
    }

    [HttpPost("refresh-token")]
    public async Task<AuthenticationResponseDto> RefreshToken(string refreshToken)
    {
        return await usersService.RefreshToken(refreshToken);
    }
}