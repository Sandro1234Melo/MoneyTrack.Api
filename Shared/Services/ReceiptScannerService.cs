using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MoneyTrack.Api.Application.Dtos.Expenses;

namespace MoneyTrack.Api.Shared.Services;

public sealed class ReceiptScannerService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ReceiptScannerService> _logger;

    public ReceiptScannerService(HttpClient http, IConfiguration configuration, ILogger<ReceiptScannerService> logger)
    {
        _http = http;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ReceiptScanResponseDto> ScanAsync(IFormFile file, CancellationToken cancellationToken)
    {
        var key = _configuration["OpenAI:ApiKey"];
        if (string.IsNullOrWhiteSpace(key))
            throw new InvalidOperationException("O scanner não está configurado. Defina OpenAI:ApiKey nos User Secrets ou numa variável de ambiente.");

        await using var source = file.OpenReadStream();
        using var memory = new MemoryStream();
        await source.CopyToAsync(memory, cancellationToken);
        var dataUrl = $"data:{file.ContentType};base64,{Convert.ToBase64String(memory.ToArray())}";

        var schema = new
        {
            type = "object",
            additionalProperties = false,
            required = new[] { "date", "merchant", "items" },
            properties = new
            {
                date = new { type = "string", description = "Data YYYY-MM-DD, ou string vazia se não estiver legível." },
                merchant = new { type = "string", description = "Nome do estabelecimento, ou string vazia." },
                items = new
                {
                    type = "array",
                    items = new
                    {
                        type = "object",
                        additionalProperties = false,
                        required = new[] { "description", "quantity", "price", "suggestedCategory" },
                        properties = new
                        {
                            description = new { type = "string" },
                            quantity = new { type = "number" },
                            price = new { type = "number", description = "Preço unitário do item, nunca o total da linha." },
                            suggestedCategory = new { type = "string", description = "Categoria curta sugerida em português." }
                        }
                    }
                }
            }
        };

        var payload = new
        {
            model = _configuration["OpenAI:ReceiptScannerModel"] ?? "gpt-4o-mini",
            store = false,
            input = new[]
            {
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new { type = "input_text", text = "Leia esta nota de compra. Extraia apenas itens efetivamente comprados; ignore subtotais, IVA, descontos, troco e total. Use ponto decimal. Se não conseguir ler algum campo, devolva string vazia (ou lista vazia para itens). Não invente dados." },
                        new { type = "input_image", image_url = dataUrl, detail = "high" }
                    }
                }
            },
            text = new { format = new { type = "json_schema", name = "receipt_scan", strict = true, schema } }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/responses")
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", key);
        using var response = await _http.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("A leitura de nota falhou com {StatusCode}", (int)response.StatusCode);
            throw new InvalidOperationException("Não foi possível ler a nota agora. Tente novamente.");
        }

        using var document = JsonDocument.Parse(body);
        var outputText = document.RootElement.TryGetProperty("output_text", out var aggregate)
            ? aggregate.GetString()
            : document.RootElement.GetProperty("output")[0].GetProperty("content")[0].GetProperty("text").GetString();
        var result = JsonSerializer.Deserialize<ReceiptScanResponseDto>(outputText ?? "", new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (result is null) throw new InvalidOperationException("A resposta do scanner não pôde ser interpretada.");

        result.Items = result.Items
            .Where(item => !string.IsNullOrWhiteSpace(item.Description) && item.Quantity > 0 && item.Price >= 0)
            .ToList();
        return result;
    }
}
