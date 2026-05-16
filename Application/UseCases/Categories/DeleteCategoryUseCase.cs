using MoneyTrack.Api.Application.Interfaces;

namespace MoneyTrack.Api.Application.UseCases.Categories
{
    public class DeleteCategoryUseCase
    {
        private readonly ICategoryRepository _repository;

        public DeleteCategoryUseCase(ICategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task Execute(int id)
        {
            var category = await _repository.GetById(id);

            if (category == null)
                throw new Exception("Categoria não encontrada");

            await _repository.Delete(category);
        }
    }
}
