using Microsoft.EntityFrameworkCore;
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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


// =====================================
// AUTOMAPPER
// =====================================

builder.Services.AddAutoMapper(typeof(AutoMapperProfile));


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


// =====================================
// MIDDLEWARE
// =====================================

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowReactApp");

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();