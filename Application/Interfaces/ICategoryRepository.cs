using MoneyTrack.Api.Domain.Entities;

namespace MoneyTrack.Api.Application.Interfaces
{
    public interface ICategoryRepository
    {
        Task<List<Category>> GetByUserId(int userId);
        Task<Category?> GetById(int id);
        Task<bool> Exists(int userId, string name);
        Task<Category> Add(Category category);
        Task Update(Category category);
        Task Delete(Category category);
    }
}
