namespace MoneyTrack.Api.Domain.Entities
{
    public class User
    {
        public int Id { get; private set; }

        public string FullName { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string PasswordHash { get; private set; } = string.Empty;

        public DateTime CreatedAt { get; private set; }

        public string Currency_Code { get; private set; } = "BRL";
        public string Country_Code { get; private set; } = "BR";
        public string Language { get; private set; } = "pt-BR";
        public string Theme { get; private set; } = "dark";
        public string DateFormat { get; private set; } = "dd/MM/yyyy";
        public string AccentColor { get; private set; } = "purple";
        public bool CompactMode { get; private set; } = false;
        public bool InterfaceAnimations { get; private set; } = true;
        public bool NotifyGoal80 { get; private set; } = true;
        public bool NotifySpendingIncrease { get; private set; } = true;
        public bool NotifyPendingLists { get; private set; } = false;
        public DateTime? LastBackupAt { get; private set; }

        public string? ProfileImageUrl { get; private set; }
        public string? BottomNavConfig { get; private set; }

        public ICollection<Category>? Categories { get; private set; }
        public ICollection<Location>? Locations { get; private set; }
        public ICollection<Expense>? Expenses { get; private set; }
        public ICollection<ShoppingList> ShoppingLists { get; private set; } = new List<ShoppingList>();

        // Construtor com validação
        public User(string fullName, string email, string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new Exception("Nome é obrigatório");

            if (string.IsNullOrWhiteSpace(email))
                throw new Exception("Email é obrigatório");

            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new Exception("Senha inválida");

            FullName = fullName;
            Email = email;
            PasswordHash = passwordHash;
            CreatedAt = DateTime.UtcNow;
        }

        private User() { }

        // Método de domínio
        public void SetPassword(string hash)
        {
            if (string.IsNullOrWhiteSpace(hash))
                throw new Exception("Hash inválido");

            PasswordHash = hash;
        }

        // Método opcional
        public void UpdateProfile(string? profileImageUrl, string? bottomNavConfig)
        {
            ProfileImageUrl = profileImageUrl;
            BottomNavConfig = bottomNavConfig;
        }

        // Método opcional
        public void UpdateAccount(string fullName, string email)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new Exception("Nome é obrigatório");

            if (string.IsNullOrWhiteSpace(email))
                throw new Exception("Email é obrigatório");

            FullName = fullName.Trim();
            Email = email.Trim();
        }

        public void UpdatePreferences(
            string currency,
            string country,
            string language,
            string theme,
            string dateFormat,
            string accentColor,
            bool compactMode,
            bool interfaceAnimations,
            bool notifyGoal80,
            bool notifySpendingIncrease,
            bool notifyPendingLists,
            string? bottomNavConfig)
        {
            Currency_Code = string.IsNullOrWhiteSpace(currency) ? Currency_Code : currency;
            Country_Code = string.IsNullOrWhiteSpace(country) ? Country_Code : country;
            Language = string.IsNullOrWhiteSpace(language) ? Language : language;
            Theme = string.IsNullOrWhiteSpace(theme) ? Theme : theme;
            DateFormat = string.IsNullOrWhiteSpace(dateFormat) ? DateFormat : dateFormat;
            AccentColor = string.IsNullOrWhiteSpace(accentColor) ? AccentColor : accentColor;
            CompactMode = compactMode;
            InterfaceAnimations = interfaceAnimations;
            NotifyGoal80 = notifyGoal80;
            NotifySpendingIncrease = notifySpendingIncrease;
            NotifyPendingLists = notifyPendingLists;
            BottomNavConfig = bottomNavConfig;
        }

        public void MarkBackupNow()
        {
            LastBackupAt = DateTime.UtcNow;
        }
        public void UpdateProfileImage(string? url)
        {
            ProfileImageUrl = url;
        }

        public void UpdatePassword(string newPasswordHash)
        {
            if (string.IsNullOrWhiteSpace(newPasswordHash))
                throw new Exception("Senha inválida");

            PasswordHash = newPasswordHash;
        }
    }
}