using MoneyTrack.Api.Application.Interfaces;
using MoneyTrack.Api.Domain.Entities;
using MoneyTrack.Api.Application.Dtos.Locations;

public class GetLocationsUseCase
{
    private readonly ILocationRepository _repository;

    public GetLocationsUseCase(ILocationRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<Location>> GetAll()
    {
        return await _repository.GetAll();
    }

    public async Task<List<Location>> GetByUser(int userId)
    {
        return await _repository.GetByUser(userId);
    }

    public async Task<List<LocationResponseDto>> GetByUserWithStats(int userId)
    {
        return await _repository.GetByUserWithStats(userId);
    }
}