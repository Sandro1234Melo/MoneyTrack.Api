using MoneyTrack.Api.Application.Dtos.Expenses;
using MoneyTrack.Api.Application.Interfaces;
using MoneyTrack.Api.Domain.Entities;

namespace MoneyTrack.Api.Application.UseCases.Expenses
{
    public class UpdateExpenseUseCase
    {
        private readonly IExpenseRepository _repository;

        public UpdateExpenseUseCase(IExpenseRepository repository)
        {
            _repository = repository;
        }

        public async Task<Expense> Execute(int id, ExpenseCreateDto dto)
        {
            var expense = await _repository.GetById(id);

            if (expense == null)
                throw new Exception("Despesa não encontrada.");

            expense.Update(
                DateTime.SpecifyKind(dto.Date, DateTimeKind.Utc),
                dto.LocationId,
                dto.PaymentMethod
            );

            expense.ClearItems();

            foreach (var itemDto in dto.Items)
            {
                var item = new ExpenseItem(
                    itemDto.Description ?? "",
                    itemDto.Quantity,
                    itemDto.Price,
                    itemDto.CategoryId
                );

                expense.AddItem(item);
            }

            await _repository.Update(expense);

            return expense;
        }
    }
}
