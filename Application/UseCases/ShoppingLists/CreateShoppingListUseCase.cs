using MoneyTrack.Api.Application.Interfaces;
using MoneyTrack.Api.Domain.Entities;
using ShoppingListEntity = MoneyTrack.Api.Domain.Entities.ShoppingList;
using MoneyTrack.Api.Application.Dtos.ShoppinLists;

namespace MoneyTrack.Api.Application.UseCases.ShoppingLists
{
    public class CreateShoppingListUseCase
    {
        private readonly IShoppingListRepository _repository;

        public CreateShoppingListUseCase(IShoppingListRepository repository)
        {
            _repository = repository;
        }

        public async Task<ShoppingListEntity> Execute(ShoppingListCreateDto dto)
        {
            var list = new ShoppingListEntity(
                dto.Name,
                dto.UserId,
                dto.PlannedDate ?? DateTime.UtcNow,
                dto.LocationId
            );

            if (dto.Items != null)
            {
                foreach (var item in dto.Items)
                {
                    var newItem = new ShoppingListItem(
                        item.Description ?? "",
                        item.CategoryId,
                        item.Quantity,
                        item.Price
                    );

                    if (item.Checked)
                        newItem.Check();

                    list.AddItem(newItem);
                }
            }

            return await _repository.Add(list);
        }
    }
}