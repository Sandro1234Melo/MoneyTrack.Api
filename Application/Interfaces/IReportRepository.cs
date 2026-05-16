namespace MoneyTrack.Api.Application.Interfaces
{
    public interface IReportRepository
    {
        Task<List<object>> GetCategoryDistribution(int userId, DateTime? from, DateTime? to);
        Task<object> GetDashboardSummary(int userId, DateTime? from, DateTime? to);
        Task<List<object>> GetMonthlyExpenses(int userId, DateTime? from, DateTime? to);
        Task<List<object>> GetPaymentMethods(int userId, DateTime? from, DateTime? to);
        Task<List<object>> GetExpensesSummary(
            int userId,
            DateTime? from,
            DateTime? to,
            int? categoryId,
            int? locationId,
            int? paymentMethod);
    }
}
