namespace MoneyTrack.Api.Application.Dtos.Users
{
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public string Full_Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string Currency_Code { get; set; } = "BRL";
        public string Country_Code { get; set; } = "BR";
        public string Language { get; set; } = "pt-BR";
        public string Theme { get; set; } = "dark";
        public string Date_Format { get; set; } = "dd/MM/yyyy";
        public string Accent_Color { get; set; } = "purple";

        public bool Compact_Mode { get; set; }
        public bool Interface_Animations { get; set; } = true;
        public bool Notify_Goal_80 { get; set; } = true;
        public bool Notify_Spending_Increase { get; set; } = true;
        public bool Notify_Pending_Lists { get; set; }

        public string? Profile_Image_Url { get; set; }
        public string? Bottom_Nav_Config { get; set; }

        public DateTime Created_At { get; set; }
        public DateTime? Last_Backup_At { get; set; }
    }
}
