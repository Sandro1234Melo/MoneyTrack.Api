using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MoneyTrack.Api.Application.Dtos.Expenses;
using MoneyTrack.Api.Application.UseCases.Expenses;

namespace MoneyTrack.Api.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExpensesController : ControllerBase
    {
        private readonly CreateExpenseUseCase _createUseCase;
        private readonly GetExpensesUseCase _getUseCase;
        private readonly UpdateExpenseUseCase _updateUseCase;
        private readonly DeleteExpenseUseCase _deleteUseCase;
        private readonly IMapper _mapper;

        public ExpensesController(
            CreateExpenseUseCase createUseCase,
            GetExpensesUseCase getUseCase,
            UpdateExpenseUseCase updateUseCase,
            DeleteExpenseUseCase deleteUseCase,
            IMapper mapper)
        {
            _createUseCase = createUseCase;
            _getUseCase = getUseCase;
            _updateUseCase = updateUseCase;
            _deleteUseCase = deleteUseCase;
            _mapper = mapper;
        }

        private IActionResult HandleError(Exception ex, string action)
        {
            return BadRequest(new
            {
                error = ex.Message,
                details = ex.InnerException?.Message,
                controller = nameof(ExpensesController),
                action
            });
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            try
            {
                var expenses = await _getUseCase.GetByUser(userId);
                var response = _mapper.Map<List<ExpenseResponseDto>>(expenses);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(GetByUser));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] ExpenseFilterDto filter)
        {
            try
            {
                var expenses = await _getUseCase.Execute(filter);
                var response = _mapper.Map<List<ExpenseResponseDto>>(expenses);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(Get));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(ExpenseCreateDto dto)
        {
            try
            {
                var expense = await _createUseCase.Execute(dto);
                return Ok(_mapper.Map<ExpenseResponseDto>(expense));
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(Create));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ExpenseCreateDto dto)
        {
            try
            {
                var expense = await _updateUseCase.Execute(id, dto);
                return Ok(_mapper.Map<ExpenseResponseDto>(expense));
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(Update));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _deleteUseCase.Execute(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(Delete));
            }
        }
    }
}
