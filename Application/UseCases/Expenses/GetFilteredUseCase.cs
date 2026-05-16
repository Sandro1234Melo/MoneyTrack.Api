using MoneyTrack.Api.Application.Dtos.Expenses;
using MoneyTrack.Api.Application.Interfaces;
using MoneyTrack.Api.Domain.Entities;

namespace MoneyTrack.Api.Application.UseCases.Expenses
{
    public class GetFilteredUseCase
    {
        private readonly IExpenseRepository _repository;

        public GetFilteredUseCase(IExpenseRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Expense>> Execute(ExpenseFilterDto filter)
        {
            return await _repository.GetFiltered(filter);
        }
    }
}