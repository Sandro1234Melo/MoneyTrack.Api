using MoneyTrack.Api.Application.Interfaces;

namespace MoneyTrack.Api.Application.UseCases.Users
{
    public class DeleteUserPhotoUseCase
    {
        private readonly IUserRepository _repository;

        public DeleteUserPhotoUseCase(IUserRepository repository)
        {
            _repository = repository;
        }

        public async Task Execute(int userId)
        {
            var user = await _repository.GetById(userId);

            if (user == null)
                throw new Exception("Usuário não encontrado");

            user.UpdateProfileImage(null);

            await _repository.Update(user);
        }
    }
}