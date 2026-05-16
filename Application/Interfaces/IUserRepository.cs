using MoneyTrack.Api.Domain.Entities;

namespace MoneyTrack.Api.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> ExistsByEmail(string email);

        Task<User?> GetByEmail(string email);

        Task<User?> GetById(int? id);

        Task<User> Add(User user);

        Task Update(User user);

        Task Delete(User user);

    }
}
