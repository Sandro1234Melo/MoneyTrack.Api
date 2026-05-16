using MoneyTrack.Api.Application.Dtos.Expenses;
using MoneyTrack.Api.Application.Interfaces;
using MoneyTrack.Api.Domain.Entities;

public class GetExpensesUseCase
{
    private readonly IExpenseRepository _repository;

    public GetExpensesUseCase(IExpenseRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<Expense>> GetByUser(int userId)
    {
        return await _repository.GetByUser(userId);
    }

    public async Task<List<Expense>> Execute(ExpenseFilterDto filter)
    {
        return await _repository.GetFiltered(filter);
    }
}