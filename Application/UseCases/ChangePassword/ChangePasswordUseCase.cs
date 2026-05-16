using MoneyTrack.Api.Application.Interfaces;
using MoneyTrack.Api.Shared.Services;

namespace MoneyTrack.Api.Application.UseCases.ChangePassword
{
    public class ChangePasswordUseCase
    {
        private readonly IUserRepository _repository;
        private readonly AuthService _authService;

        public ChangePasswordUseCase(
            IUserRepository repository,
            AuthService authService)
        {
            _repository = repository;
            _authService = authService;
        }

        public async Task Execute(
            int userId,
            string currentPassword,
            string newPassword)
        {
            var user = await _repository.GetById(userId);

            if (user == null)
                throw new Exception("Usuário não encontrado");

            if (!_authService.VerifyPassword(
                currentPassword,
                user.PasswordHash))
            {
                throw new Exception("Senha atual inválida");
            }

            var newHash = _authService.HashPassword(newPassword);

            user.UpdatePassword(newHash);

            await _repository.Update(user);
        }
    }
}