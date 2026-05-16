using System.ComponentModel.DataAnnotations;

namespace MoneyTrack.Api.Application.Dtos.Categories
{
    public class CategoryCreateDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int User_Id { get; set; }
    }
}
