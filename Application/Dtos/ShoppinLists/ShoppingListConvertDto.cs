using MoneyTrack.Api.Domain.Enum;

namespace MoneyTrack.Api.Application.Dtos.ShoppinLists
{
    public class ShoppingListConvertDto
    {
        public PaymentMethodEnum PaymentMethod { get; set; } = PaymentMethodEnum.Cash;
        public int? LocationId { get; set; }
    }
}
