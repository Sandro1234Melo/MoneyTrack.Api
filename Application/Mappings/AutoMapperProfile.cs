using AutoMapper;
using MoneyTrack.Api.Application.Dtos.Categories;
using MoneyTrack.Api.Application.Dtos.ExpenseItem;
using MoneyTrack.Api.Application.Dtos.Expenses;
using MoneyTrack.Api.Application.Dtos.Locations;
using MoneyTrack.Api.Application.Dtos.ShoppinLists;
using MoneyTrack.Api.Application.Dtos.Users;
using MoneyTrack.Api.Domain.Entities;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // USERS
        CreateMap<User, UserResponseDto>()
            .ForMember(dest => dest.Full_Name, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.Currency_Code, opt => opt.MapFrom(src => src.Currency_Code))
            .ForMember(dest => dest.Country_Code, opt => opt.MapFrom(src => src.Country_Code))
            .ForMember(dest => dest.Date_Format, opt => opt.MapFrom(src => src.DateFormat))
            .ForMember(dest => dest.Accent_Color, opt => opt.MapFrom(src => src.AccentColor))
            .ForMember(dest => dest.Compact_Mode, opt => opt.MapFrom(src => src.CompactMode))
            .ForMember(dest => dest.Interface_Animations, opt => opt.MapFrom(src => src.InterfaceAnimations))
            .ForMember(dest => dest.Notify_Goal_80, opt => opt.MapFrom(src => src.NotifyGoal80))
            .ForMember(dest => dest.Notify_Spending_Increase, opt => opt.MapFrom(src => src.NotifySpendingIncrease))
            .ForMember(dest => dest.Notify_Pending_Lists, opt => opt.MapFrom(src => src.NotifyPendingLists))
            .ForMember(dest => dest.Last_Backup_At, opt => opt.MapFrom(src => src.LastBackupAt))
            .ForMember(dest => dest.Created_At, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.Profile_Image_Url,
                opt => opt.MapFrom(src => src.ProfileImageUrl))
            .ForMember(dest => dest.Bottom_Nav_Config,
                opt => opt.MapFrom(src => src.BottomNavConfig));

        CreateMap<UserCreateDto, User>()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

        // CATEGORIES
        CreateMap<Category, CategoryResponseDto>();
        CreateMap<CategoryCreateDto, Category>();

        // LOCATIONS
        CreateMap<Location, LocationResponseDto>();
        CreateMap<LocationCreateDto, Location>();

        // EXPENSES
        CreateMap<ExpenseCreateDto, Expense>();

        CreateMap<Expense, ExpenseResponseDto>()
            .ForMember(dest => dest.Amount,
                opt => opt.MapFrom(src => src.Items.Sum(i => i.Amount)))
            .ForMember(dest => dest.Items,
                opt => opt.MapFrom(src => src.Items));

        // EXPENSE ITEMS
        CreateMap<ExpenseItemCreateDto, ExpenseItem>();
        CreateMap<ExpenseItem, ExpenseItemResponseDto>();

        // SHOPPING LIST
        CreateMap<ShoppingList, ShoppingListResponseDto>()
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.LocationName,
                opt => opt.MapFrom(src => src.Location != null ? src.Location.Name : null))
            .ForMember(dest => dest.TotalItems,
                opt => opt.MapFrom(src => src.Items.Count))
            .ForMember(dest => dest.CheckedItems,
                opt => opt.MapFrom(src => src.Items.Count(i => i.Checked)))
            .ForMember(dest => dest.EstimatedTotal,
                opt => opt.MapFrom(src => src.Items.Sum(i => (i.Price ?? 0) * i.Quantity)))
            .ForMember(dest => dest.ProgressPercent,
                opt => opt.MapFrom(src => src.Items.Count == 0 ? 0 : Math.Round((decimal)src.Items.Count(i => i.Checked) * 100 / src.Items.Count, 2)));

        // SHOPPING LIST ITEM
        CreateMap<ShoppingListItem, ShoppingListItemResponseDto>()
            .ForMember(dest => dest.CategoryName,
                opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty))
            .ForMember(dest => dest.Total,
                opt => opt.MapFrom(src => (src.Price ?? 0) * src.Quantity));
    }
}