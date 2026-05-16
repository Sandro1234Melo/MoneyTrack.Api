using MoneyTrack.Api.Application.Dtos.ShoppinLists;
using MoneyTrack.Api.Application.Interfaces;
using MoneyTrack.Api.Domain.Entities;

namespace MoneyTrack.Api.Application.UseCases.ShoppingLists
{
    public class ConvertShoppingListUseCase
    {
        private readonly IShoppingListRepository _repository;
        private readonly IExpenseRepository _expenseRepository;

        public ConvertShoppingListUseCase(
            IShoppingListRepository repository,
            IExpenseRepository expenseRepository)
        {
            _repository = repository;
            _expenseRepository = expenseRepository;
        }

        public async Task<int> Execute(int id, ShoppingListConvertDto? dto = null)
        {
            var list = await _repository.GetById(id);

            if (list == null)
                throw new Exception("Lista não encontrada");

            if (list.Status == Domain.Enum.ShoppingListStatusEnum.Converted)
                throw new Exception("Lista já convertida");

            if (!list.Items.Any())
                throw new Exception("Adicione itens antes de converter a lista em gasto.");

            var itemsToConvert = list.Items.Any(i => i.Checked)
                ? list.Items.Where(i => i.Checked).ToList()
                : list.Items.ToList();

            var expense = new Expense(
                DateTime.UtcNow,
                list.UserId,
                dto?.PaymentMethod ?? Domain.Enum.PaymentMethodEnum.Cash,
                dto?.LocationId ?? list.LocationId
            );

            foreach (var item in itemsToConvert)
            {
                var unitPrice = item.Price ?? 0;

                expense.AddItem(new ExpenseItem(
                    item.Description,
                    item.Quantity,
                    unitPrice,
                    item.CategoryId
                ));
            }

            list.MarkAsConverted();

            await _expenseRepository.Add(expense);
            await _repository.Update(list);

            return expense.Id;
        }
    }
}
