using MoneyTrack.Api.Application.Interfaces;

namespace MoneyTrack.Api.Application.UseCases.Reports
{
    public class GetCategoryDistributionUseCase
    {
        private readonly IReportRepository _repository;

        public GetCategoryDistributionUseCase(IReportRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<object>> Execute(
            int userId,
            DateTime? from,
            DateTime? to)
        {
            return await _repository.GetCategoryDistribution(userId, from, to);
        }
    }
}