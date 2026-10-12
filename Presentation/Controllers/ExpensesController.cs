using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;
using MoneyTrack.Api.Application.Dtos.Expenses;
using MoneyTrack.Api.Application.UseCases.Expenses;
using MoneyTrack.Api.Application.Interfaces;
using MoneyTrack.Api.Domain.Entities;
using MoneyTrack.Api.Domain.Enum;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

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

        public ExpensesController(
            CreateExpenseUseCase createUseCase,
            GetExpensesUseCase getUseCase,
            UpdateExpenseUseCase updateUseCase,
            DeleteExpenseUseCase deleteUseCase,
            IMapper mapper,
            IExpenseRepository repository,
            IWebHostEnvironment environment)
        {
            _createUseCase = createUseCase;
            _getUseCase = getUseCase;
            _updateUseCase = updateUseCase;
            _deleteUseCase = deleteUseCase;
            _mapper = mapper;
            _repository = repository;
            _environment = environment;
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

            return Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(24);
                    page.DefaultTextStyle(style => style.FontSize(9));

                    page.Header().Column(column =>
                    {
                        column.Item().Text("Relatório de gastos").FontSize(18).Bold();
                        column.Item().Text($"Gerado em {DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC").FontColor(Colors.Grey.Darken1);
                    });

                    page.Content().PaddingTop(16).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(54);
                            columns.ConstantColumn(34);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.ConstantColumn(50);
                            columns.ConstantColumn(60);
                        });

                        table.Header(header =>
                        {
                            foreach (var title in new[] { "Data", "Nota", "Produto", "Categoria", "Pagamento", "Local", "Qtd.", "Valor" })
                                header.Cell().Element(HeaderCell).Text(title).Bold();
                        });

                        foreach (var row in rows)
                        {
                            table.Cell().Element(BodyCell).Text(row.Date.ToString("dd/MM/yyyy"));
                            table.Cell().Element(BodyCell).Text(row.ExpenseId.ToString());
                            table.Cell().Element(BodyCell).Text(row.Description);
                            table.Cell().Element(BodyCell).Text(row.Category);
                            table.Cell().Element(BodyCell).Text(row.PaymentMethod);
                            table.Cell().Element(BodyCell).Text(row.Location);
                            table.Cell().Element(BodyCell).AlignRight().Text(row.Quantity.ToString("0.##"));
                            table.Cell().Element(BodyCell).AlignRight().Text(row.Amount.ToString("N2"));
                        }

                        if (rows.Count == 0)
                            table.Cell().ColumnSpan(8).Padding(12).Text("Nenhuma despesa encontrada para os filtros selecionados.");
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("MoneyTrack • Página ");
                        text.CurrentPageNumber();
                        text.Span(" de ");
                        text.TotalPages();
                    });
                });
            }).GeneratePdf();
        }

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

        private static IContainer HeaderCell(IContainer container) =>
            container.Background(Colors.Blue.Darken3).Padding(5).DefaultTextStyle(style => style.FontColor(Colors.White));

        private static IContainer BodyCell(IContainer container) =>
            container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(4).PaddingHorizontal(2);

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
