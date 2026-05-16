using MoneyTrack.Api.Application.Dtos.Locations;
using MoneyTrack.Api.Application.Interfaces;
using MoneyTrack.Api.Domain.Entities;

public class UpdateLocationUseCase
{
    private readonly ILocationRepository _repository;

    public UpdateLocationUseCase(ILocationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Location> Execute(int id, LocationCreateDto dto)
    {
        var location = await _repository.GetById(id);

        if (location == null)
            throw new Exception("Localização não encontrada");

        location.Update(dto.Name, dto.User_Id);

        await _repository.Update(location);

        return location;
    }
}