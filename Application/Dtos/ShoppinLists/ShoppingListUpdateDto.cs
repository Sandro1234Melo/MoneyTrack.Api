using System.ComponentModel.DataAnnotations;

namespace MoneyTrack.Api.Application.Dtos.ShoppinLists
{
    public class ShoppingListUpdateDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public int? LocationId { get; set; }
        public DateTime? PlannedDate { get; set; }
    }
}
