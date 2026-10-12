using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MoneyTrack.Api.Application.Interfaces;
using MoneyTrack.Api.Application.UseCases.Auth;
using MoneyTrack.Api.Application.UseCases.Categories;
using MoneyTrack.Api.Application.UseCases.ChangePassword;
using MoneyTrack.Api.Application.UseCases.Expenses;
using MoneyTrack.Api.Application.UseCases.Reports;
using MoneyTrack.Api.Application.UseCases.ShoppingLists;
using MoneyTrack.Api.Application.UseCases.Users;
using MoneyTrack.Api.Data.Repositories;
using MoneyTrack.Api.Infrastructure.Data;
using MoneyTrack.Api.Shared.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// =====================================
// DATABASE
// =====================================

builder.Services.AddDbContext<MoneyTrackDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);


// =====================================
// CORS
// =====================================

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
    throw new InvalidOperationException("Configure Jwt:Key com pelo menos 32 bytes em User Secrets ou variável de ambiente.");

var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "MoneyTrack.Api";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "MoneyTrack.Web";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:5173", "https://moneytrack-frontend-alpha.vercel.app" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


// =====================================
// AUTOMAPPER
// =====================================

builder.Services.AddAutoMapper(_ => { }, typeof(AutoMapperProfile));


// =====================================
// SERVICES
// =====================================

builder.Services.AddScoped<AuthService>();


// =====================================
// REPOSITORIES
// =====================================

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();
builder.Services.AddScoped<IShoppingListRepository, ShoppingListRepository>();
builder.Services.AddScoped<ILocationRepository, LocationRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();

// =====================================
// AUTH
// =====================================

builder.Services.AddScoped<RegisterUserUseCase>();
builder.Services.AddScoped<LoginUserUseCase>();
builder.Services.AddScoped<ChangePasswordUseCase>();


// =====================================
// USERS
// =====================================

builder.Services.AddScoped<GetUserMeUseCase>();
builder.Services.AddScoped<UpdateUserPreferencesUseCase>();
builder.Services.AddScoped<UploadUserPhotoUseCase>();
builder.Services.AddScoped<DeleteUserPhotoUseCase>();


// =====================================
// CATEGORIES
// =====================================

builder.Services.AddScoped<CreateCategoryUseCase>();
builder.Services.AddScoped<GetCategoriesByUserUseCase>();
builder.Services.AddScoped<UpdateCategoryUseCase>();
builder.Services.AddScoped<DeleteCategoryUseCase>();


// =====================================
// EXPENSES
// =====================================

builder.Services.AddScoped<CreateExpenseUseCase>();
builder.Services.AddScoped<GetExpensesUseCase>();
builder.Services.AddScoped<GetFilteredUseCase>();
builder.Services.AddScoped<UpdateExpenseUseCase>();
builder.Services.AddScoped<DeleteExpenseUseCase>();

// =====================================
// REPORTS
// =====================================

builder.Services.AddScoped<GetCategoryDistributionUseCase>();
builder.Services.AddScoped<GetDashboardSummaryUseCase>();
builder.Services.AddScoped<GetMonthlyExpensesUseCase>();
builder.Services.AddScoped<GetPaymentMethodsUseCase>();
builder.Services.AddScoped<GetExpensesSummaryUseCase>();


// =====================================
// LOCATIONS
// =====================================

builder.Services.AddScoped<GetLocationsUseCase>();
builder.Services.AddScoped<CreateLocationUseCase>();
builder.Services.AddScoped<UpdateLocationUseCase>();
builder.Services.AddScoped<DeleteLocationUseCase>();


// =====================================
// SHOPPING LISTS
// =====================================

builder.Services.AddScoped<CreateShoppingListUseCase>();
builder.Services.AddScoped<GetShoppingListsByUserUseCase>();
builder.Services.AddScoped<ConvertShoppingListUseCase>();
builder.Services.AddScoped<DeleteShoppingListUseCase>();


// =====================================
// CONTROLLERS
// =====================================

builder.Services.AddControllers();


// =====================================
// SWAGGER
// =====================================

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Mantém o banco compatível com o campo de comprovante usado pelas despesas.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MoneyTrackDbContext>();
    await db.Database.ExecuteSqlRawAsync(
        "ALTER TABLE expenses ADD COLUMN IF NOT EXISTS receipt_image_url varchar(500) NULL;"
    );
}

var profileDirectory = Path.Combine(app.Environment.ContentRootPath, "wwwroot", "profile");
var receiptDirectory = Path.Combine(app.Environment.ContentRootPath, "wwwroot", "receipts");
Directory.CreateDirectory(profileDirectory);
Directory.CreateDirectory(receiptDirectory);


// =====================================
// MIDDLEWARE
// =====================================

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowReactApp");

app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(profileDirectory),
    RequestPath = "/profile"
});
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(receiptDirectory),
    RequestPath = "/receipts"
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers().RequireAuthorization();

app.Run();
