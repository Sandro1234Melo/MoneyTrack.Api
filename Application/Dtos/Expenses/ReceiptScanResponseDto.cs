namespace MoneyTrack.Api.Application.Dtos.Expenses;

/// <summary>Dados extraídos de uma nota. Devem ser confirmados pelo utilizador antes de criar a compra.</summary>
public sealed class ReceiptScanResponseDto
{
    public string Date { get; set; } = "";
    public string Merchant { get; set; } = "";
    public List<ReceiptScanItemDto> Items { get; set; } = new();
}

public sealed class ReceiptScanItemDto
{
    public string Description { get; set; } = "";
    public decimal Quantity { get; set; } = 1;
    public decimal Price { get; set; }
    public string SuggestedCategory { get; set; } = "";
}
