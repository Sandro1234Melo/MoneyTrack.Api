using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyTrack.Api.Application.Dtos.Users;
using MoneyTrack.Api.Application.Interfaces;
using MoneyTrack.Api.Application.UseCases.ChangePassword;
using MoneyTrack.Api.Application.UseCases.Users;
using MoneyTrack.Api.Infrastructure.Data;

namespace MoneyTrack.Api.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : AuthenticatedControllerBase
    {
        private readonly GetUserMeUseCase _getMeUseCase;
        private readonly UpdateUserPreferencesUseCase _updatePreferencesUseCase;
        private readonly UploadUserPhotoUseCase _uploadPhotoUseCase;
        private readonly DeleteUserPhotoUseCase _deletePhotoUseCase;
        private readonly ChangePasswordUseCase _changePasswordUseCase;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly MoneyTrackDbContext _context;

        public UsersController(
            GetUserMeUseCase getMeUseCase,
            UpdateUserPreferencesUseCase updatePreferencesUseCase,
            UploadUserPhotoUseCase uploadPhotoUseCase,
            DeleteUserPhotoUseCase deletePhotoUseCase,
            ChangePasswordUseCase changePasswordUseCase,
            IUserRepository userRepository,
            IMapper mapper,
            MoneyTrackDbContext context)
        {
            _getMeUseCase = getMeUseCase;
            _updatePreferencesUseCase = updatePreferencesUseCase;
            _uploadPhotoUseCase = uploadPhotoUseCase;
            _deletePhotoUseCase = deletePhotoUseCase;
            _changePasswordUseCase = changePasswordUseCase;
            _userRepository = userRepository;
            _mapper = mapper;
            _context = context;
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

        private int GetUserId() => CurrentUserId;

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

                var exportData = new
                {
                    ExportedAt = DateTime.UtcNow,
                    Profile = new
                    {
                        user.FullName,
                        user.Email,
                        user.CreatedAt,
                        user.Currency_Code,
                        user.Country_Code,
                        user.Language,
                        user.Theme,
                        user.DateFormat,
                        user.AccentColor,
                        user.CompactMode,
                        user.InterfaceAnimations,
                        user.NotifyGoal80,
                        user.NotifySpendingIncrease,
                        user.NotifyPendingLists,
                        user.LastBackupAt,
                    },
                    Categories = await _context.Categories
                        .AsNoTracking()
                        .Where(category => category.UserId == userId)
                        .OrderBy(category => category.Name)
                        .Select(category => new { category.Id, category.Name })
                        .ToListAsync(),
                    Locations = await _context.Locations
                        .AsNoTracking()
                        .Where(location => location.UserId == userId)
                        .OrderBy(location => location.Name)
                        .Select(location => new { location.Id, location.Name })
                        .ToListAsync(),
                    Expenses = await _context.Expenses
                        .AsNoTracking()
                        .Include(expense => expense.Location)
                        .Include(expense => expense.Items)
                        .ThenInclude(item => item.Category)
                        .Where(expense => expense.UserId == userId)
                        .OrderBy(expense => expense.Date)
                        .Select(expense => new
                        {
                            expense.Id,
                            expense.Date,
                            expense.PaymentMethod,
                            Location = expense.Location == null ? null : expense.Location.Name,
                            expense.ReceiptImageUrl,
                            Items = expense.Items.Select(item => new
                            {
                                item.Description,
                                Category = item.Category.Name,
                                item.Quantity,
                                item.UnitPrice,
                                item.Amount,
                            }).ToList(),
                        })
                        .ToListAsync(),
                    Goals = await _context.Goals
                        .AsNoTracking()
                        .Include(goal => goal.Category)
                        .Include(goal => goal.Location)
                        .Where(goal => goal.UserId == userId)
                        .OrderBy(goal => goal.CreatedAt)
                        .Select(goal => new
                        {
                            goal.Name,
                            goal.Description,
                            goal.Type,
                            goal.Scope,
                            goal.Period,
                            goal.TargetAmount,
                            goal.SavedAmount,
                            Category = goal.Category == null ? null : goal.Category.Name,
                            Location = goal.Location == null ? null : goal.Location.Name,
                            goal.StartDate,
                            goal.EndDate,
                            goal.AlertPercentage,
                            goal.IsActive,
                            goal.IsFavorite,
                            goal.CreatedAt,
                            goal.UpdatedAt,
                        })
                        .ToListAsync(),
                    ShoppingLists = await _context.ShoppingLists
                        .AsNoTracking()
                        .Include(list => list.Location)
                        .Include(list => list.Items)
                        .ThenInclude(item => item.Category)
                        .Where(list => list.UserId == userId)
                        .OrderBy(list => list.CreatedAt)
                        .Select(list => new
                        {
                            list.Name,
                            Location = list.Location == null ? null : list.Location.Name,
                            list.PlannedDate,
                            list.Status,
                            list.CreatedAt,
                            list.UpdatedAt,
                            Items = list.Items.Select(item => new
                            {
                                item.Description,
                                Category = item.Category.Name,
                                item.Quantity,
                                item.Price,
                                item.Checked,
                            }).ToList(),
                        })
                        .ToListAsync(),
                    Friendships = await _context.Friendships
                        .AsNoTracking()
                        .Where(friendship => friendship.SenderId == userId || friendship.ReceiverId == userId)
                        .OrderBy(friendship => friendship.CreatedAt)
                        .Select(friendship => new
                        {
                            friendship.SenderId,
                            friendship.ReceiverId,
                            friendship.Status,
                            friendship.CreatedAt,
                            friendship.RespondedAt,
                        })
                        .ToListAsync(),
                };

                var fileName = $"moneytrack-dados-usuario-{userId}-{DateTime.UtcNow:yyyyMMddHHmmss}.json";
                var json = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(exportData, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });
                return File(json, "application/json", fileName);
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
