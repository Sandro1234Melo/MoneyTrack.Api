using MoneyTrack.Api.Application.Dtos.Expenses;
using MoneyTrack.Api.Application.Interfaces;
using MoneyTrack.Api.Domain.Entities;

public class CreateExpenseUseCase
{
    private readonly IExpenseRepository _repository;

    public CreateExpenseUseCase(IExpenseRepository repository)
    {
        _repository = repository;
    }

    public async Task<Expense> Execute(ExpenseCreateDto dto)
    {
        var expense = new Expense(
            DateTime.SpecifyKind(dto.Date, DateTimeKind.Utc),
            dto.UserId,
            dto.PaymentMethod,
            dto.LocationId
            
        );

        foreach (var itemDto in dto.Items)
        {
            if (itemDto.CategoryId <= 0)
                throw new Exception("Item com categoria inválida.");

            var item = new ExpenseItem(
                itemDto.Description ?? string.Empty,
                itemDto.Quantity,
                itemDto.Price,
                itemDto.CategoryId
            );

            expense.AddItem(item);
        }

        return await _repository.Add(expense);
    }
}