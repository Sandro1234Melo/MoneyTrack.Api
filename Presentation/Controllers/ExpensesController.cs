using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;
using MoneyTrack.Api.Application.Dtos.Expenses;
using MoneyTrack.Api.Application.UseCases.Expenses;
using MoneyTrack.Api.Application.Interfaces;
using MoneyTrack.Api.Domain.Entities;
using MoneyTrack.Api.Domain.Enum;
using MoneyTrack.Api.Shared.Services;
using System.Globalization;
using System.Text;

namespace MoneyTrack.Api.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExpensesController : AuthenticatedControllerBase
    {
        private readonly CreateExpenseUseCase _createUseCase;
        private readonly GetExpensesUseCase _getUseCase;
        private readonly UpdateExpenseUseCase _updateUseCase;
        private readonly DeleteExpenseUseCase _deleteUseCase;
        private readonly IMapper _mapper;
        private readonly IExpenseRepository _repository;
        private readonly IWebHostEnvironment _environment;
        private readonly ReceiptScannerService _receiptScanner;

        public ExpensesController(
            CreateExpenseUseCase createUseCase,
            GetExpensesUseCase getUseCase,
            UpdateExpenseUseCase updateUseCase,
            DeleteExpenseUseCase deleteUseCase,
            IMapper mapper,
            IExpenseRepository repository,
            IWebHostEnvironment environment,
            ReceiptScannerService receiptScanner)
        {
            _createUseCase = createUseCase;
            _getUseCase = getUseCase;
            _updateUseCase = updateUseCase;
            _deleteUseCase = deleteUseCase;
            _mapper = mapper;
            _repository = repository;
            _environment = environment;
            _receiptScanner = receiptScanner;
        }

        private IActionResult HandleError(Exception ex, string action)
        {
            return BadRequest(new
            {
                error = ex.Message,
                details = ex.InnerException?.Message,
                controller = nameof(ExpensesController),
                action
            });
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            try
            {
                if (userId != CurrentUserId) return Forbid();
                var expenses = await _getUseCase.GetByUser(userId);
                var response = _mapper.Map<List<ExpenseResponseDto>>(expenses);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(GetByUser));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] ExpenseFilterDto filter)
        {
            try
            {
                filter.UserId = CurrentUserId;
                var expenses = await _getUseCase.Execute(filter);
                var response = _mapper.Map<List<ExpenseResponseDto>>(expenses);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(Get));
            }
        }

        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportExcel([FromQuery] ExpenseFilterDto filter)
        {
            try
            {
                filter.UserId = CurrentUserId;
                var expenses = await _getUseCase.Execute(filter);
                var content = BuildExcel(expenses);

                return File(
                    content,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    BuildFileName("xlsx"));
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(ExportExcel));
            }
        }

        [HttpGet("export/pdf")]
        public async Task<IActionResult> ExportPdf([FromQuery] ExpenseFilterDto filter)
        {
            try
            {
                filter.UserId = CurrentUserId;
                var expenses = await _getUseCase.Execute(filter);
                var content = BuildPdf(expenses);

                return File(content, "application/pdf", BuildFileName("pdf"));
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(ExportPdf));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(ExpenseCreateDto dto)
        {
            try
            {
                dto.UserId = CurrentUserId;
                var expense = await _createUseCase.Execute(dto);
                return Ok(_mapper.Map<ExpenseResponseDto>(expense));
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(Create));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ExpenseCreateDto dto)
        {
            try
            {
                var existingExpense = await _repository.GetById(id);
                if (existingExpense == null || existingExpense.UserId != CurrentUserId)
                    return NotFound(new { error = "Compra não encontrada." });

                dto.UserId = CurrentUserId;
                var expense = await _updateUseCase.Execute(id, dto);
                return Ok(_mapper.Map<ExpenseResponseDto>(expense));
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(Update));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var existingExpense = await _repository.GetById(id);
                if (existingExpense == null || existingExpense.UserId != CurrentUserId)
                    return NotFound(new { error = "Compra não encontrada." });

                await _deleteUseCase.Execute(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return HandleError(ex, nameof(Delete));
            }
        }

        [HttpPost("{id}/receipt")]
        public async Task<IActionResult> UploadReceipt(int id, IFormFile file)
        {
            try
            {
                var userId = CurrentUserId;
                if (file == null || file.Length == 0 || file.Length > 10 * 1024 * 1024 || !file.ContentType.StartsWith("image/"))
                    return BadRequest(new { error = "Envie uma imagem de até 10 MB." });

                var expense = await _repository.GetById(id);
                if (expense == null || expense.UserId != userId) return NotFound(new { error = "Compra não encontrada." });

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (extension is not ".jpg" and not ".jpeg" and not ".png" and not ".webp")
                    return BadRequest(new { error = "Formato aceito: JPG, PNG ou WEBP." });

                var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
                var directory = Path.Combine(webRoot, "receipts");
                Directory.CreateDirectory(directory);
                var fileName = $"{Guid.NewGuid():N}{extension}";
                await using (var stream = System.IO.File.Create(Path.Combine(directory, fileName))) await file.CopyToAsync(stream);
                var url = $"/receipts/{fileName}";
                expense.UpdateReceiptImage(url);
                await _repository.Update(expense);
                return Ok(new { url });
            }
            catch (Exception ex) { return HandleError(ex, nameof(UploadReceipt)); }
        }

        [HttpPost("scan-receipt")]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<IActionResult> ScanReceipt(IFormFile file, CancellationToken cancellationToken)
        {
            try
            {
                if (file == null || file.Length == 0 || file.Length > 10 * 1024 * 1024 || !file.ContentType.StartsWith("image/"))
                    return BadRequest(new { error = "Envie uma imagem de até 10 MB." });
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (extension is not ".jpg" and not ".jpeg" and not ".png" and not ".webp")
                    return BadRequest(new { error = "Formato aceito: JPG, PNG ou WEBP." });

                return Ok(await _receiptScanner.ScanAsync(file, cancellationToken));
            }
            catch (Exception ex) { return HandleError(ex, nameof(ScanReceipt)); }
        }

        private static byte[] BuildExcel(IEnumerable<Expense> expenses)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Gastos");
            var rows = Flatten(expenses).ToList();

            worksheet.Cell(1, 1).Value = "Relatório de gastos — MoneyTrack";
            worksheet.Range(1, 1, 1, 8).Merge();
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;
            worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            var headers = new[] { "Data", "Nota", "Produto", "Categoria", "Pagamento", "Local", "Quantidade", "Valor" };
            for (var column = 0; column < headers.Length; column++)
            {
                var cell = worksheet.Cell(3, column + 1);
                cell.Value = headers[column];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("312E81");
                cell.Style.Font.FontColor = XLColor.White;
            }

            var rowNumber = 4;
            foreach (var row in rows)
            {
                worksheet.Cell(rowNumber, 1).Value = row.Date;
                worksheet.Cell(rowNumber, 1).Style.DateFormat.Format = "dd/MM/yyyy";
                worksheet.Cell(rowNumber, 2).Value = row.ExpenseId;
                worksheet.Cell(rowNumber, 3).Value = row.Description;
                worksheet.Cell(rowNumber, 4).Value = row.Category;
                worksheet.Cell(rowNumber, 5).Value = row.PaymentMethod;
                worksheet.Cell(rowNumber, 6).Value = row.Location;
                worksheet.Cell(rowNumber, 7).Value = row.Quantity;
                worksheet.Cell(rowNumber, 8).Value = row.Amount;
                worksheet.Cell(rowNumber, 8).Style.NumberFormat.Format = "#,##0.00";
                rowNumber++;
            }

            if (rows.Count > 0)
                worksheet.Range(3, 1, rowNumber - 1, 8).CreateTable();

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        private static byte[] BuildPdf(IEnumerable<Expense> expenses)
        {
            var rows = Flatten(expenses).ToList();
            var lines = rows.Count == 0
                ? new List<string> { "Nenhuma despesa encontrada para os filtros selecionados." }
                : rows.Select(row => string.Join(" | ", new[]
                {
                    row.Date.ToString("dd/MM/yyyy"),
                    $"#{row.ExpenseId}",
                    Shorten(row.Description, 24),
                    Shorten(row.Category, 16),
                    Shorten(row.PaymentMethod, 18),
                    Shorten(row.Location, 16),
                    row.Quantity.ToString("0.##", CultureInfo.InvariantCulture),
                    row.Amount.ToString("N2", CultureInfo.InvariantCulture)
                })).ToList();

            const int rowsPerPage = 39;
            var pages = lines.Chunk(rowsPerPage).ToList();
            return BuildSimplePdf(pages);
        }

        private static byte[] BuildSimplePdf(IReadOnlyList<string[]> pages)
        {
            var objects = new List<string>();
            var pageIds = Enumerable.Range(0, pages.Count).Select(index => 4 + index * 2).ToArray();
            objects.Add("<< /Type /Catalog /Pages 2 0 R >>");
            objects.Add($"<< /Type /Pages /Kids [{string.Join(" ", pageIds.Select(id => $"{id} 0 R"))}] /Count {pages.Count} >>");
            objects.Add("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>");

            for (var index = 0; index < pages.Count; index++)
            {
                var content = BuildPageContent(pages[index], index + 1, pages.Count);
                var contentBytes = Encoding.ASCII.GetByteCount(content);
                var pageId = pageIds[index];
                var contentId = pageId + 1;
                objects.Add($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 842 595] /Resources << /Font << /F1 3 0 R >> >> /Contents {contentId} 0 R >>");
                objects.Add($"<< /Length {contentBytes} >>\nstream\n{content}\nendstream");
            }

            var builder = new StringBuilder("%PDF-1.4\n%\u00e2\u00e3\u00cf\u00d3\n");
            var offsets = new List<int> { 0 };
            for (var index = 0; index < objects.Count; index++)
            {
                offsets.Add(Encoding.ASCII.GetByteCount(builder.ToString()));
                builder.Append($"{index + 1} 0 obj\n{objects[index]}\nendobj\n");
            }

            var xrefOffset = Encoding.ASCII.GetByteCount(builder.ToString());
            builder.Append($"xref\n0 {objects.Count + 1}\n0000000000 65535 f \n");
            foreach (var offset in offsets.Skip(1)) builder.Append($"{offset:0000000000} 00000 n \n");
            builder.Append($"trailer\n<< /Size {objects.Count + 1} /Root 1 0 R >>\nstartxref\n{xrefOffset}\n%%EOF");
            return Encoding.ASCII.GetBytes(builder.ToString());
        }

        private static string BuildPageContent(IEnumerable<string> rows, int page, int pageCount)
        {
            var lines = new List<string>
            {
                "Relatorio de gastos",
                $"Gerado em {DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC",
                "Data | Nota | Produto | Categoria | Pagamento | Local | Qtd. | Valor",
                "--------------------------------------------------------------------------------"
            };
            lines.AddRange(rows);
            lines.Add($"Pagina {page} de {pageCount}");
            return $"BT\n/F1 10 Tf\n50 545 Td\n12 TL\n{string.Join("\n", lines.Select(line => $"({PdfEscape(line)}) Tj T*"))}\nET";
        }

        private static string PdfEscape(string value) => ToAscii(value).Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");

        private static string ToAscii(string value) => string.Concat(value.Normalize(NormalizationForm.FormD)
            .Where(character => CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            .Select(character => character <= 127 ? character : '?'));

        private static string Shorten(string value, int maxLength) => value.Length <= maxLength ? value : $"{value[..Math.Max(0, maxLength - 1)]}…";

        private static IEnumerable<ExportRow> Flatten(IEnumerable<Expense> expenses) =>
            expenses.SelectMany(expense => expense.Items.Select(item => new ExportRow(
                expense.Date,
                expense.Id,
                item.Description,
                item.Category?.Name ?? "Sem categoria",
                PaymentMethodLabel(expense.PaymentMethod),
                expense.Location?.Name ?? "Sem local",
                item.Quantity,
                item.Amount)));

        private static string PaymentMethodLabel(PaymentMethodEnum paymentMethod) => paymentMethod switch
        {
            PaymentMethodEnum.Cash => "Dinheiro",
            PaymentMethodEnum.DebitCard => "Cartão de débito",
            PaymentMethodEnum.CreditCard => "Cartão de crédito",
            PaymentMethodEnum.Pix => "PIX",
            PaymentMethodEnum.BankTransfer => "Transferência bancária",
            PaymentMethodEnum.Mbway => "MB Way",
            PaymentMethodEnum.ValeAlimentacao => "Vale alimentação",
            _ => "Não informado"
        };

        private static string BuildFileName(string extension) =>
            $"moneytrack-gastos-{DateTime.UtcNow:yyyyMMdd-HHmmss}.{extension}";

        private sealed record ExportRow(
            DateTime Date,
            int ExpenseId,
            string Description,
            string Category,
            string PaymentMethod,
            string Location,
            decimal Quantity,
            decimal Amount);
    }
}
