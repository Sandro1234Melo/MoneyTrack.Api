using MoneyTrack.Api.Application.Interfaces;

namespace MoneyTrack.Api.Application.UseCases.Expenses
{
    public class DeleteExpenseUseCase
    {
        private readonly IExpenseRepository _repository;

        public DeleteExpenseUseCase(IExpenseRepository repository)
        {
            _repository = repository;
        }

        public async Task Execute(int id)
        {
            var expense = await _repository.GetById(id);

            if (expense == null)
                throw new Exception("Despesa não encontrada.");

            await _repository.Delete(expense);
        }
    }
}
