namespace ACommerce.Templates.Customer.Pages;

public class AddressModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? Region { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; } = "المملكة العربية السعودية";
    public bool IsDefault { get; set; }
}

public class PaymentMethodModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Icon { get; set; } = "bi-credit-card";
    public bool IsEnabled { get; set; } = true;
}

public class PlaceOrderEventArgs
{
    public Guid? AddressId { get; set; }
    public AddressModel? NewAddress { get; set; }
    public string? PaymentMethodId { get; set; }
    public bool SaveAddress { get; set; }
}
