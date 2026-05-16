using MoneyTrack.Api.Application.Dtos.Expenses;
using MoneyTrack.Api.Domain.Entities;

namespace MoneyTrack.Api.Application.Interfaces
{
    public interface IExpenseRepository
    {
        Task<List<Expense>> GetByUser(int userId);
        Task<List<Expense>> GetFiltered(ExpenseFilterDto filter);
        Task<Expense?> GetById(int id);
        Task<Expense> Add(Expense expense);
        Task Update(Expense expense);
        Task Delete(Expense expense);
    }
}
