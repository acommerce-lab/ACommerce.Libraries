namespace ACommerce.Authentication.Abstractions;

public class AuthenticationException : Exception
{
	public string ErrorCode { get; }

	public AuthenticationException(string errorCode, string message)
		: base(message)
	{
		ErrorCode = errorCode;
	}

	public AuthenticationException(string errorCode, string message, Exception innerException)
		: base(message, innerException)
	{
		ErrorCode = errorCode;
	}
}

