using Microsoft.AspNetCore.Mvc;
using MoneyTrack.Api.Application.UseCases.Reports;

namespace MoneyTrack.Api.Presentation.Controllers
{
    [ApiController]
    [Route("api/reports")]
    public class ReportsController : ControllerBase
    {
        private readonly GetCategoryDistributionUseCase _categoryUseCase;
        private readonly GetDashboardSummaryUseCase _dashboardUseCase;
        private readonly GetMonthlyExpensesUseCase _monthlyUseCase;
        private readonly GetPaymentMethodsUseCase _paymentUseCase;
        private readonly GetExpensesSummaryUseCase _summaryUseCase;

        public ReportsController(
            GetCategoryDistributionUseCase categoryUseCase,
            GetDashboardSummaryUseCase dashboardUseCase,
            GetMonthlyExpensesUseCase monthlyUseCase,
            GetPaymentMethodsUseCase paymentUseCase,
            GetExpensesSummaryUseCase summaryUseCase)
        {
            _categoryUseCase = categoryUseCase;
            _dashboardUseCase = dashboardUseCase;
            _monthlyUseCase = monthlyUseCase;
            _paymentUseCase = paymentUseCase;
            _summaryUseCase = summaryUseCase;
        }

        private IActionResult HandleError(Exception ex, string action)
        {
            return BadRequest(new
            {
                error = ex.Message,
                details = ex.InnerException?.Message,
                controller = nameof(ReportsController),
                action
            });
        }

        [HttpGet("category-distribution")]
        public async Task<IActionResult> CategoryDistribution(
            [FromQuery] int userId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            try
            {
                if (userId <= 0)
                    return BadRequest(new { error = "UserId inválido" });

                var result = await _categoryUseCase.Execute(userId, from, to);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(CategoryDistribution));
            }
        }

        [HttpGet("dashboard-summary")]
        public async Task<IActionResult> Dashboard(
            [FromQuery] int userId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            try
            {
                if (userId <= 0)
                    return BadRequest(new { error = "UserId inválido" });

                var result = await _dashboardUseCase.Execute(userId, from, to);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(Dashboard));
            }
        }

        [HttpGet("monthly-expenses")]
        public async Task<IActionResult> Monthly(
            [FromQuery] int userId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            try
            {
                if (userId <= 0)
                    return BadRequest(new { error = "UserId inválido" });

                var result = await _monthlyUseCase.Execute(userId, from, to);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(Monthly));
            }
        }

        [HttpGet("payment-methods")]
        public async Task<IActionResult> PaymentMethods(
            [FromQuery] int userId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            try
            {
                if (userId <= 0)
                    return BadRequest(new { error = "UserId inválido" });

                var result = await _paymentUseCase.Execute(userId, from, to);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(PaymentMethods));
            }
        }

        [HttpGet("expenses-summary")]
        public async Task<IActionResult> Summary(
            [FromQuery] int userId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] int? categoryId,
            [FromQuery] int? locationId,
            [FromQuery] int? paymentMethod)
        {
            try
            {
                if (userId <= 0)
                    return BadRequest(new { error = "UserId inválido" });

                var result = await _summaryUseCase.Execute(
                    userId, from, to, categoryId, locationId, paymentMethod);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(Summary));
            }
        }
    }
}
