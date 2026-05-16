using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MoneyTrack.Api.Application.Dtos.Users;
using MoneyTrack.Api.Application.Interfaces;
using MoneyTrack.Api.Application.UseCases.ChangePassword;
using MoneyTrack.Api.Application.UseCases.Users;

namespace MoneyTrack.Api.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly GetUserMeUseCase _getMeUseCase;
        private readonly UpdateUserPreferencesUseCase _updatePreferencesUseCase;
        private readonly UploadUserPhotoUseCase _uploadPhotoUseCase;
        private readonly DeleteUserPhotoUseCase _deletePhotoUseCase;
        private readonly ChangePasswordUseCase _changePasswordUseCase;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UsersController(
            GetUserMeUseCase getMeUseCase,
            UpdateUserPreferencesUseCase updatePreferencesUseCase,
            UploadUserPhotoUseCase uploadPhotoUseCase,
            DeleteUserPhotoUseCase deletePhotoUseCase,
            ChangePasswordUseCase changePasswordUseCase,
            IUserRepository userRepository,
            IMapper mapper)
        {
            _getMeUseCase = getMeUseCase;
            _updatePreferencesUseCase = updatePreferencesUseCase;
            _uploadPhotoUseCase = uploadPhotoUseCase;
            _deletePhotoUseCase = deletePhotoUseCase;
            _changePasswordUseCase = changePasswordUseCase;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        private IActionResult HandleError(Exception ex, string action, int statusCode = 400)
        {
            return StatusCode(statusCode, new
            {
                error = ex.Message,
                details = ex.InnerException?.Message,
                controller = nameof(UsersController),
                action
            });
        }

        private int GetUserId()
        {
            if (Request.Headers.TryGetValue("X-User-Id", out var value))
            {
                if (int.TryParse(value, out var id))
                    return id;
            }

            throw new Exception("Usuário não autenticado");
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            try
            {
                var userId = GetUserId();
                var user = await _getMeUseCase.Execute(userId);
                var dto = _mapper.Map<UserResponseDto>(user);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(GetMe), StatusCodes.Status401Unauthorized);
            }
        }

        [HttpPut("me/preferences")]
        public async Task<IActionResult> UpdatePreferences([FromBody] UserPreferencesDto dto)
        {
            try
            {
                var userId = GetUserId();
                await _updatePreferencesUseCase.Execute(userId, dto);
                var user = await _getMeUseCase.Execute(userId);
                return Ok(_mapper.Map<UserResponseDto>(user));
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(UpdatePreferences));
            }
        }

        [HttpPost("me/upload-photo")]
        public async Task<IActionResult> UploadPhoto(IFormFile file)
        {
            try
            {
                var userId = GetUserId();
                var url = await _uploadPhotoUseCase.Execute(userId, file);
                return Ok(new { url });
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(UploadPhoto));
            }
        }

        [HttpDelete("me/profile-photo")]
        public async Task<IActionResult> DeletePhoto()
        {
            try
            {
                var userId = GetUserId();
                await _deletePhotoUseCase.Execute(userId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(DeletePhoto));
            }
        }

        [HttpPut("me/password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            try
            {
                var userId = GetUserId();

                await _changePasswordUseCase.Execute(
                    userId,
                    dto.CurrentPassword,
                    dto.NewPassword
                );

                return Ok(new { message = "Senha alterada com sucesso" });
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(ChangePassword));
            }
        }

        [HttpGet("me/export")]
        public async Task<IActionResult> ExportMyData()
        {
            try
            {
                var userId = GetUserId();
                var user = await _userRepository.GetById(userId);

                if (user == null)
                    return NotFound(new { message = "Usuário não encontrado" });

                user.MarkBackupNow();
                await _userRepository.Update(user);

                var dto = _mapper.Map<UserResponseDto>(user);
                var fileName = $"moneytrack-dados-usuario-{userId}-{DateTime.UtcNow:yyyyMMddHHmmss}.json";
                return File(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(dto), "application/json", fileName);
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(ExportMyData));
            }
        }

        [HttpDelete("me")]
        public async Task<IActionResult> DeleteMyAccount()
        {
            try
            {
                var userId = GetUserId();
                var user = await _userRepository.GetById(userId);

                if (user == null)
                    return NotFound(new { message = "Usuário não encontrado" });

                await _userRepository.Delete(user);
                return Ok(new { message = "Conta apagada com sucesso" });
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(DeleteMyAccount));
            }
        }
    }
}
