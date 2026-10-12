using MoneyTrack.Api.Application.Interfaces;

namespace MoneyTrack.Api.Application.UseCases.Reports
{
    public class GetMonthlyExpensesUseCase
    {
        private readonly IReportRepository _repository;

        public GetMonthlyExpensesUseCase(IReportRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<object>> Execute(
            int userId,
            DateTime? from,
            DateTime? to,
            int? categoryId,
            int? locationId,
            int? paymentMethod)
        {
            return await _repository.GetMonthlyExpenses(
                userId, from, to, categoryId, locationId, paymentMethod);
        }
    }
}
