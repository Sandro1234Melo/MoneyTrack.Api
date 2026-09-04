using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MoneyTrack.Api.Application.Dtos.Users;
using MoneyTrack.Api.Application.UseCases.Auth;
using MoneyTrack.Api.Shared.Utils;

namespace MoneyTrack.Api.Presentation.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly RegisterUserUseCase _registerUseCase;
        private readonly LoginUserUseCase _loginUseCase;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public AuthController(
            RegisterUserUseCase registerUseCase,
            LoginUserUseCase loginUseCase,
            IMapper mapper,
            IConfiguration configuration)
        {
            _registerUseCase = registerUseCase;
            _loginUseCase = loginUseCase;
            _mapper = mapper;
            _configuration = configuration;
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

        private UserResponseDto CreateAuthenticatedResponse(Domain.Entities.User user)
        {
            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(jwtKey))
                throw new InvalidOperationException("A chave JWT não está configurada.");

            if (System.Text.Encoding.UTF8.GetByteCount(jwtKey) < 32)
                throw new InvalidOperationException("A chave JWT deve ter pelo menos 32 bytes.");

            var response = _mapper.Map<UserResponseDto>(user);
            response.Token = JwtTokenGenerator.GenerateToken(user, jwtKey);
            return response;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserCreateDto dto)
        {
            try
            {
                var user = await _registerUseCase.Execute(dto);
                var response = CreateAuthenticatedResponse(user);
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
                var response = CreateAuthenticatedResponse(user);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return HandleError(ex, nameof(Login), StatusCodes.Status401Unauthorized);
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(Login), StatusCodes.Status500InternalServerError);
            }
        }
    }
}
