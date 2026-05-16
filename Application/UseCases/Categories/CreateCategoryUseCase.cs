using MoneyTrack.Api.Application.Interfaces;
using MoneyTrack.Api.Domain.Entities;

namespace MoneyTrack.Api.Application.UseCases.Categories
{
    public class CreateCategoryUseCase
    {
        private readonly ICategoryRepository _repository;

        public CreateCategoryUseCase(ICategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<Category> Execute(string name, int userId)
        {
            if (await _repository.Exists(userId, name))
                throw new Exception("Categoria já existe");

            var category = new Category(name, userId);

            return await _repository.Add(category);
        }
    }
}
