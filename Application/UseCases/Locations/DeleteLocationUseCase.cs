using MoneyTrack.Api.Application.Interfaces;

public class DeleteLocationUseCase
{
    private readonly ILocationRepository _repository;

    public DeleteLocationUseCase(ILocationRepository repository)
    {
        _repository = repository;
    }

    public async Task Execute(int id)
    {
        var location = await _repository.GetById(id);

        if (location == null)
            throw new Exception("Localização não encontrada");

        await _repository.Delete(location);
    }
}