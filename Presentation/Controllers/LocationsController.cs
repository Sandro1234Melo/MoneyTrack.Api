using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MoneyTrack.Api.Application.Dtos.Locations;

namespace MoneyTrack.Api.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocationsController : ControllerBase
    {
        private readonly GetLocationsUseCase _getUseCase;
        private readonly CreateLocationUseCase _createUseCase;
        private readonly UpdateLocationUseCase _updateUseCase;
        private readonly DeleteLocationUseCase _deleteUseCase;
        private readonly IMapper _mapper;

        public LocationsController(
            GetLocationsUseCase getUseCase,
            CreateLocationUseCase createUseCase,
            UpdateLocationUseCase updateUseCase,
            DeleteLocationUseCase deleteUseCase,
            IMapper mapper)
        {
            _getUseCase = getUseCase;
            _createUseCase = createUseCase;
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
                controller = nameof(LocationsController),
                action
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var locations = await _getUseCase.GetAll();
                return Ok(_mapper.Map<IEnumerable<LocationResponseDto>>(locations));
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(GetAll));
            }
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            try
            {
                var locations = await _getUseCase.GetByUserWithStats(userId);
                return Ok(locations);
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(GetByUser));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(LocationCreateDto dto)
        {
            try
            {
                var location = await _createUseCase.Execute(dto);
                return Ok(_mapper.Map<LocationResponseDto>(location));
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(Create));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, LocationCreateDto dto)
        {
            try
            {
                var location = await _updateUseCase.Execute(id, dto);
                return Ok(_mapper.Map<LocationResponseDto>(location));
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
