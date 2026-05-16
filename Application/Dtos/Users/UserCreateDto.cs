using System.ComponentModel.DataAnnotations;

namespace MoneyTrack.Api.Application.Dtos.Users
{
    public class UserCreateDto
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [StringLength(3)]
        public string CurrencyCode { get; set; } = "BRL";

        [Required]
        [StringLength(2)]
        public string CountryCode { get; set; } = "BR";

        [Required]
        public string Language { get; set; } = "pt-BR";
    }
}