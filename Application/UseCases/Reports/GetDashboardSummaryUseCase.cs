using MoneyTrack.Api.Application.Interfaces;

namespace MoneyTrack.Api.Application.UseCases.Reports
{
    public class GetDashboardSummaryUseCase
    {
        private readonly IReportRepository _repository;

        public GetDashboardSummaryUseCase(IReportRepository repository)
        {
            _repository = repository;
        }

        public async Task<object> Execute(int userId, DateTime? from, DateTime? to)
        {
            return await _repository.GetDashboardSummary(userId, from, to);
        }
    }
}
