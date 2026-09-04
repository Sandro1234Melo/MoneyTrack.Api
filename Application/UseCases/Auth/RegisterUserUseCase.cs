using MoneyTrack.Api.Application.Dtos.Users;
using MoneyTrack.Api.Application.Interfaces;
using MoneyTrack.Api.Domain.Entities;
using MoneyTrack.Api.Shared.Services;

namespace MoneyTrack.Api.Application.UseCases.Auth
{
    public class RegisterUserUseCase
    {
        private readonly IUserRepository _repository;
        private readonly AuthService _authService;

        public RegisterUserUseCase(
            IUserRepository repository,
            AuthService authService)
        {
            _repository = repository;
            _authService = authService;
        }

        public async Task<User> Execute(UserCreateDto dto)
        {
            var exists = await _repository.ExistsByEmail(dto.Email);

            if (exists)
                throw new Exception("Email já cadastrado.");

            var hash = _authService.HashPassword(dto.Password);

            var user = new User(dto.FullName, dto.Email, hash);

            user.UpdatePreferences(
                dto.CurrencyCode,
                dto.CountryCode,
                dto.Language,
                "dark",
                "dd/MM/yyyy",
                "purple",
                false,
                true,
                true,
                true,
                false,
                null);

            return await _repository.Add(user);
        }
    }
}
