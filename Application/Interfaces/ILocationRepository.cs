using MoneyTrack.Api.Domain.Entities;
using MoneyTrack.Api.Application.Dtos.Locations;

namespace MoneyTrack.Api.Application.Interfaces
{
    public interface ILocationRepository
    {
        Task<List<Location>> GetAll();
        Task<List<Location>> GetByUser(int userId);
        Task<List<LocationResponseDto>> GetByUserWithStats(int userId);
        Task<Location?> GetById(int id);
        Task<Location> Add(Location location);
        Task Update(Location location);
        Task Delete(Location location);
        Task<bool> Exists(int userId, string name);

    }
}
