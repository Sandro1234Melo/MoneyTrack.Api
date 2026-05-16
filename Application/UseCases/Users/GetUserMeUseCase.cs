using MoneyTrack.Api.Application.Interfaces;
using MoneyTrack.Api.Domain.Entities;

namespace MoneyTrack.Api.Application.UseCases.Users
{
    public class GetUserMeUseCase
    {
        private readonly IUserRepository _repository;

        public GetUserMeUseCase(IUserRepository repository)
        {
            _repository = repository;
        }

        public async Task<User> Execute(int userId)
        {
            var user = await _repository.GetById(userId);

            if (user == null)
                throw new Exception("Usuário não encontrado");

            return user;
        }
    }
}