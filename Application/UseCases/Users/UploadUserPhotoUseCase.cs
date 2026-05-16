using MoneyTrack.Api.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace MoneyTrack.Api.Application.UseCases.Users
{
    public class UploadUserPhotoUseCase
    {
        private readonly IUserRepository _repository;

        public UploadUserPhotoUseCase(IUserRepository repository)
        {
            _repository = repository;
        }

        public async Task<string> Execute(int userId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new Exception("Arquivo inválido");

            var user = await _repository.GetById(userId);

            if (user == null)
                throw new Exception("Usuário não encontrado");

            var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/profile");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(folder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            var url = $"/profile/{fileName}";

            user.UpdateProfileImage(url);

            await _repository.Update(user);

            return url;
        }
    }
}