using MoneyTrack.Api.Application.Interfaces;

namespace MoneyTrack.Api.Application.UseCases.Categories
{
    public class UpdateCategoryUseCase
    {
        private readonly ICategoryRepository _repository;

        public UpdateCategoryUseCase(ICategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task Execute(int id, string name)
        {
            var category = await _repository.GetById(id);

            if (category == null)
                throw new Exception("Categoria não encontrada");

            category.UpdateName(name);

            await _repository.Update(category);
        }
    }
}
