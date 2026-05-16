using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MoneyTrack.Api.Application.Dtos.Categories;
using MoneyTrack.Api.Application.UseCases.Categories;

namespace MoneyTrack.Api.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly CreateCategoryUseCase _createUseCase;
        private readonly GetCategoriesByUserUseCase _getUseCase;
        private readonly UpdateCategoryUseCase _updateUseCase;
        private readonly DeleteCategoryUseCase _deleteUseCase;
        private readonly IMapper _mapper;

        public CategoriesController(
            CreateCategoryUseCase createUseCase,
            GetCategoriesByUserUseCase getUseCase,
            UpdateCategoryUseCase updateUseCase,
            DeleteCategoryUseCase deleteUseCase,
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
                controller = nameof(CategoriesController),
                action
            });
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            try
            {
                var categories = await _getUseCase.Execute(userId);
                var response = _mapper.Map<IEnumerable<CategoryResponseDto>>(categories);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(GetByUser));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(CategoryCreateDto dto)
        {
            try
            {
                var category = await _createUseCase.Execute(dto.Name, dto.User_Id);
                var response = _mapper.Map<CategoryResponseDto>(category);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(Create));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CategoryUpdateDto dto)
        {
            try
            {
                await _updateUseCase.Execute(id, dto.Name);
                return NoContent();
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
