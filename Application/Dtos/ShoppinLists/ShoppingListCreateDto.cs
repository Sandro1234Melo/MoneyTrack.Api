using System.ComponentModel.DataAnnotations;

namespace MoneyTrack.Api.Application.Dtos.ShoppinLists
{
    public class ShoppingListCreateDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;
        public int? LocationId { get; set; }

        public DateTime? PlannedDate { get; set; }

        [Required]
        public List<ShoppingListItemCreateDto> Items { get; set; } = new();
    }
}