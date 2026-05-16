using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MoneyTrack.Api.Application.Dtos.ShoppinLists;
using MoneyTrack.Api.Application.Interfaces;
using MoneyTrack.Api.Application.UseCases.ShoppingLists;
using MoneyTrack.Api.Domain.Entities;

namespace MoneyTrack.Api.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShoppingListsController : ControllerBase
    {
        private readonly GetShoppingListsByUserUseCase _getUseCase;
        private readonly CreateShoppingListUseCase _createUseCase;
        private readonly ConvertShoppingListUseCase _convertUseCase;
        private readonly DeleteShoppingListUseCase _deleteUseCase;
        private readonly IShoppingListRepository _repository;
        private readonly IMapper _mapper;

        public ShoppingListsController(
            GetShoppingListsByUserUseCase getUseCase,
            CreateShoppingListUseCase createUseCase,
            ConvertShoppingListUseCase convertUseCase,
            DeleteShoppingListUseCase deleteUseCase,
            IShoppingListRepository repository,
            IMapper mapper)
        {
            _getUseCase = getUseCase;
            _createUseCase = createUseCase;
            _convertUseCase = convertUseCase;
            _deleteUseCase = deleteUseCase;
            _repository = repository;
            _mapper = mapper;
        }

        private IActionResult HandleError(Exception ex, string action)
        {
            var details = ex.InnerException?.Message;
            var inner = ex.InnerException;

            while (inner?.InnerException != null)
            {
                inner = inner.InnerException;
                details = string.IsNullOrWhiteSpace(details)
                    ? inner.Message
                    : $"{details} | {inner.Message}";
            }

            return BadRequest(new
            {
                error = ex.Message,
                details,
                controller = nameof(ShoppingListsController),
                action
            });
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(int userId, [FromQuery] string? search, [FromQuery] string? status)
        {
            try
            {
                var lists = await _getUseCase.Execute(userId, search, status);
                return Ok(_mapper.Map<List<ShoppingListResponseDto>>(lists));
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(GetByUser));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var list = await _repository.GetById(id);

                if (list == null)
                    return NotFound(new { message = "Lista não encontrada" });

                return Ok(_mapper.Map<ShoppingListResponseDto>(list));
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(GetById));
            }
        }

        [HttpGet("user/{userId}/summary")]
        public async Task<IActionResult> Summary(int userId)
        {
            try
            {
                var lists = await _getUseCase.Execute(userId);
                var dto = new ShoppingListSummaryDto
                {
                    TotalLists = lists.Count,
                    PendingItems = lists.SelectMany(l => l.Items).Count(i => !i.Checked),
                    EstimatedTotal = lists.SelectMany(l => l.Items).Sum(i => (i.Price ?? 0) * i.Quantity),
                    PotentialSavings = Math.Round(lists.SelectMany(l => l.Items).Sum(i => (i.Price ?? 0) * i.Quantity) * 0.08m, 2)
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(Summary));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ShoppingListCreateDto dto)
        {
            try
            {
                var list = await _createUseCase.Execute(dto);
                return CreatedAtAction(nameof(GetById), new { id = list.Id }, _mapper.Map<ShoppingListResponseDto>(list));
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(Create));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ShoppingListUpdateDto dto)
        {
            try
            {
                var list = await _repository.GetById(id);

                if (list == null)
                    return NotFound(new { message = "Lista não encontrada" });

                list.Update(dto.Name, dto.PlannedDate ?? list.PlannedDate, dto.LocationId);
                await _repository.Update(list);

                return Ok(_mapper.Map<ShoppingListResponseDto>(list));
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(Update));
            }
        }

        [HttpPost("{id}/items")]
        public async Task<IActionResult> AddItem(int id, [FromBody] ShoppingListItemCreateDto dto)
        {
            try
            {
                var list = await _repository.GetById(id);

                if (list == null)
                    return NotFound(new { message = "Lista não encontrada" });

                var item = new ShoppingListItem(dto.Description ?? string.Empty, dto.CategoryId, dto.Quantity, dto.Price, dto.Checked);
                list.AddItem(item);
                list.UpdateStatusFromItems();
                await _repository.Update(list);

                return Ok(_mapper.Map<ShoppingListResponseDto>(list));
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(AddItem));
            }
        }

        [HttpPut("{id}/items/{itemId}")]
        public async Task<IActionResult> UpdateItem(int id, int itemId, [FromBody] ShoppingListItemUpdateDto dto)
        {
            try
            {
                var list = await _repository.GetById(id);

                if (list == null)
                    return NotFound(new { message = "Lista não encontrada" });

                var item = list.Items.FirstOrDefault(i => i.Id == itemId);

                if (item == null)
                    return NotFound(new { message = "Item não encontrado" });

                item.Update(dto.Description ?? string.Empty, dto.CategoryId, dto.Quantity, dto.Price);
                item.SetChecked(dto.Checked);
                list.UpdateStatusFromItems();
                await _repository.Update(list);

                return Ok(_mapper.Map<ShoppingListResponseDto>(list));
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(UpdateItem));
            }
        }

        [HttpPatch("{id}/items/{itemId}/check")]
        public async Task<IActionResult> CheckItem(int id, int itemId, [FromBody] ShoppingListItemCheckDto dto)
        {
            try
            {
                var list = await _repository.GetById(id);

                if (list == null)
                    return NotFound(new { message = "Lista não encontrada" });

                var item = list.Items.FirstOrDefault(i => i.Id == itemId);

                if (item == null)
                    return NotFound(new { message = "Item não encontrado" });

                item.SetChecked(dto.Checked);
                list.UpdateStatusFromItems();
                await _repository.Update(list);

                return Ok(_mapper.Map<ShoppingListResponseDto>(list));
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(CheckItem));
            }
        }

        [HttpDelete("{id}/items/{itemId}")]
        public async Task<IActionResult> DeleteItem(int id, int itemId)
        {
            try
            {
                var list = await _repository.GetById(id);

                if (list == null)
                    return NotFound(new { message = "Lista não encontrada" });

                var item = list.Items.FirstOrDefault(i => i.Id == itemId);

                if (item == null)
                    return NotFound(new { message = "Item não encontrado" });

                list.RemoveItem(item);
                await _repository.Update(list);

                return Ok(_mapper.Map<ShoppingListResponseDto>(list));
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(DeleteItem));
            }
        }

        [HttpPost("{id}/execute")]
        public async Task<IActionResult> Execute(int id)
        {
            try
            {
                var list = await _repository.GetById(id);

                if (list == null)
                    return NotFound(new { message = "Lista não encontrada" });

                list.Start();
                await _repository.Update(list);

                return Ok(_mapper.Map<ShoppingListResponseDto>(list));
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(Execute));
            }
        }

        [HttpPost("{id}/convert")]
        public async Task<IActionResult> Convert(int id, [FromBody] ShoppingListConvertDto? dto)
        {
            try
            {
                var expenseId = await _convertUseCase.Execute(id, dto);
                return Ok(new
                {
                    message = "Lista convertida com sucesso",
                    expenseId
                });
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(Convert));
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
