namespace ACommerce.Authentication.TwoFactor.SMS;

public interface ISmsProvider
{
    Task SendAsync(
        string phoneNumber,
        string message,
        CancellationToken cancellationToken = default);
}