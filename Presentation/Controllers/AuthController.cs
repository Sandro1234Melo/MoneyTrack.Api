using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MoneyTrack.Api.Application.Dtos.Users;
using MoneyTrack.Api.Application.UseCases.Auth;

namespace MoneyTrack.Api.Presentation.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly RegisterUserUseCase _registerUseCase;
        private readonly LoginUserUseCase _loginUseCase;
        private readonly IMapper _mapper;

        public AuthController(
            RegisterUserUseCase registerUseCase,
            LoginUserUseCase loginUseCase,
            IMapper mapper)
        {
            _registerUseCase = registerUseCase;
            _loginUseCase = loginUseCase;
            _mapper = mapper;
        }

        private IActionResult HandleError(Exception ex, string action, int statusCode = 400)
        {
            return StatusCode(statusCode, new
            {
                error = ex.Message,
                details = ex.InnerException?.Message,
                controller = nameof(AuthController),
                action
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserCreateDto dto)
        {
            try
            {
                var user = await _registerUseCase.Execute(dto);
                var response = _mapper.Map<UserResponseDto>(user);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(Register));
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
        {
            try
            {
                var user = await _loginUseCase.Execute(dto.Email, dto.Password);
                var response = _mapper.Map<UserResponseDto>(user);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(Login), StatusCodes.Status401Unauthorized);
            }
        }
    }
}
