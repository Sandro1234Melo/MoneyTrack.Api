using MoneyTrack.Api.Domain.Entities;

namespace MoneyTrack.Api.Application.Interfaces
{
    public interface IShoppingListRepository
    {
        Task<List<ShoppingList>> GetByUser(int userId, string? search = null, string? status = null);
        Task<ShoppingList?> GetById(int id);
        Task<ShoppingList> Add(ShoppingList list);
        Task Update(ShoppingList list);
        Task Delete(ShoppingList list);
    }
}
