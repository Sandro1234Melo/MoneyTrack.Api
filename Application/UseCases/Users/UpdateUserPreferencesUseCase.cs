using MoneyTrack.Api.Application.Dtos.Users;
using MoneyTrack.Api.Application.Interfaces;

namespace MoneyTrack.Api.Application.UseCases.Users
{
    public class UpdateUserPreferencesUseCase
    {
        private readonly IUserRepository _repository;

        public UpdateUserPreferencesUseCase(IUserRepository repository)
        {
            _repository = repository;
        }

        public async Task Execute(int userId, UserPreferencesDto dto)
        {
            var user = await _repository.GetById(userId);

            if (user == null)
                throw new Exception("Usuário não encontrado");

            user.UpdateAccount(
                string.IsNullOrWhiteSpace(dto.Full_Name) ? user.FullName : dto.Full_Name,
                string.IsNullOrWhiteSpace(dto.Email) ? user.Email : dto.Email
            );

            user.UpdatePreferences(
                dto.Currency_Code,
                dto.Country_Code,
                dto.Language,
                dto.Theme,
                dto.Date_Format,
                dto.Accent_Color,
                dto.Compact_Mode,
                dto.Interface_Animations,
                dto.Notify_Goal_80,
                dto.Notify_Spending_Increase,
                dto.Notify_Pending_Lists,
                dto.Bottom_Nav_Config
            );

            await _repository.Update(user);
        }
    }
}
