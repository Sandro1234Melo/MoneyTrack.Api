using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MoneyTrack.Api.Application.Dtos.Categories;
using MoneyTrack.Api.Application.Interfaces;
using MoneyTrack.Api.Application.UseCases.Categories;

namespace MoneyTrack.Api.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : AuthenticatedControllerBase
    {
        private readonly CreateCategoryUseCase _createUseCase;
        private readonly GetCategoriesByUserUseCase _getUseCase;
        private readonly UpdateCategoryUseCase _updateUseCase;
        private readonly DeleteCategoryUseCase _deleteUseCase;
        private readonly IMapper _mapper;
        private readonly ICategoryRepository _repository;

        public CategoriesController(
            CreateCategoryUseCase createUseCase,
            GetCategoriesByUserUseCase getUseCase,
            UpdateCategoryUseCase updateUseCase,
            DeleteCategoryUseCase deleteUseCase,
            IMapper mapper,
            ICategoryRepository repository)
        {
            _createUseCase = createUseCase;
            _getUseCase = getUseCase;
            _updateUseCase = updateUseCase;
            _deleteUseCase = deleteUseCase;
            _mapper = mapper;
            _repository = repository;
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
                if (userId != CurrentUserId) return Forbid();
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
                var category = await _createUseCase.Execute(dto.Name, CurrentUserId);
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
                var category = await _repository.GetById(id);
                if (category == null || category.UserId != CurrentUserId)
                    return NotFound(new { error = "Categoria não encontrada." });
                if (string.IsNullOrWhiteSpace(dto.Name))
                    return BadRequest(new { error = "Informe o nome da categoria." });

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
                var category = await _repository.GetById(id);
                if (category == null || category.UserId != CurrentUserId)
                    return NotFound(new { error = "Categoria não encontrada." });

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
