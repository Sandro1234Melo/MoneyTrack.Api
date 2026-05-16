using MoneyTrack.Api.Application.Interfaces;
using MoneyTrack.Api.Domain.Entities;

namespace MoneyTrack.Api.Application.UseCases.ShoppingLists
{
    public class GetShoppingListsByUserUseCase
    {
        private readonly IShoppingListRepository _repository;

        public GetShoppingListsByUserUseCase(IShoppingListRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<ShoppingList>> Execute(int userId, string? search = null, string? status = null)
        {
            return await _repository.GetByUser(userId, search, status);
        }
    }
}