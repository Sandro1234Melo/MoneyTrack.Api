using MoneyTrack.Api.Application.Interfaces;
using MoneyTrack.Api.Domain.Entities;
using MoneyTrack.Api.Shared.Services;

public class LoginUserUseCase
{
    private readonly IUserRepository _repository;
    private readonly AuthService _authService;

    public LoginUserUseCase(
        IUserRepository repository,
        AuthService authService)
    {
        _repository = repository;
        _authService = authService;
    }

    public async Task<User> Execute(string email, string password)
    {
        var user = await _repository.GetByEmail(email);

        if (user == null)
            throw new Exception("Email ou senha inválidos.");

        var valid = _authService.VerifyPassword(password, user.PasswordHash);

        if (!valid)
            throw new Exception("Email ou senha inválidos.");

        return user;
    }
}