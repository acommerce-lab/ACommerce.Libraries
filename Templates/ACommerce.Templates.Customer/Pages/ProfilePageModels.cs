namespace ACommerce.Templates.Customer.Pages;

public class UserProfileInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime? MemberSince { get; set; }
    public bool IsVerified { get; set; }
}
