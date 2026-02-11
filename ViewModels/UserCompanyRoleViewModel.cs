namespace DigitalSignage.ViewModels
{
    public class UserCompanyRoleViewModel
    {
        public int UserCompanyRoleID { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int CompanyID { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsOffice365User { get; set; }
        public DateTime AssignedDate { get; set; }
    }
}
