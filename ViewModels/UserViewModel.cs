namespace DigitalSignage.ViewModels
{
    public class UserViewModel
    {
        public int UserID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsSystemAdmin { get; set; }
        public bool IsOffice365User { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastLoginDate { get; set; }

        // Display Properties
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string StatusText => IsActive ? "Active" : "Inactive";
    }
}
