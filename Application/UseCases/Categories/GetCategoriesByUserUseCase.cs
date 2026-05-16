using MoneyTrack.Api.Application.Interfaces;
using MoneyTrack.Api.Domain.Entities;

namespace MoneyTrack.Api.Application.UseCases.Categories
{
    public class GetCategoriesByUserUseCase
    {
        private readonly ICategoryRepository _repository;

        public GetCategoriesByUserUseCase(ICategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Category>> Execute(int userId)
        {
            return await _repository.GetByUserId(userId);
        }
    }
}
