namespace ACommerce.Authentication.Abstractions;

public class TwoFactorAuthenticationException : AuthenticationException
{
	public string? TransactionId { get; }

	public TwoFactorAuthenticationException(
		string errorCode,
		string message,
		string? transactionId = null)
		: base(errorCode, message)
	{
		TransactionId = transactionId;
	}
}

