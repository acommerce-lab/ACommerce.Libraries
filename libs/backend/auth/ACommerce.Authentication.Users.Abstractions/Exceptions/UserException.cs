// Exceptions/UserException.cs
namespace ACommerce.Authentication.Users.Abstractions.Exceptions;

public class UserException : Exception
{
	public string ErrorCode { get; }

	public UserException(string errorCode, string message)
		: base(message)
	{
		ErrorCode = errorCode;
	}

	public UserException(string errorCode, string message, Exception innerException)
		: base(message, innerException)
	{
		ErrorCode = errorCode;
	}
}

