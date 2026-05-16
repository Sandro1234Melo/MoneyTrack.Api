using MoneyTrack.Api.Application.Interfaces;

namespace MoneyTrack.Api.Application.UseCases.ShoppingLists
{
    public class DeleteShoppingListUseCase
    {
        private readonly IShoppingListRepository _repository;

        public DeleteShoppingListUseCase(IShoppingListRepository repository)
        {
            _repository = repository;
        }

        public async Task Execute(int id)
        {
            var list = await _repository.GetById(id);

            if (list == null)
                throw new Exception("Lista não encontrada");

            if (list.Status == Domain.Enum.ShoppingListStatusEnum.Converted)
                throw new Exception("Não pode excluir lista convertida");

            await _repository.Delete(list);
        }
    }
}