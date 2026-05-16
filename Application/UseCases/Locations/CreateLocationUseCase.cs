using MoneyTrack.Api.Application.Dtos.Locations;
using MoneyTrack.Api.Application.Interfaces;
using MoneyTrack.Api.Domain.Entities;

public class CreateLocationUseCase
{
    private readonly ILocationRepository _repository;

    public CreateLocationUseCase(ILocationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Location> Execute(LocationCreateDto dto)
    {
        var exists = await _repository.Exists(dto.User_Id, dto.Name);

        if (exists)
            throw new Exception("Já existe uma localização com esse nome.");

        var location = new Location(
            dto.Name,
            dto.User_Id
        );

        return await _repository.Add(location);
    }
}