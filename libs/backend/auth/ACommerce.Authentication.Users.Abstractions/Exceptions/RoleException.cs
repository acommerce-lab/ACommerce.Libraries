// Exceptions/RoleException.cs
namespace ACommerce.Authentication.Users.Abstractions.Exceptions;

// Exceptions/RoleException.cs
public class RoleException : Exception
{
	public string ErrorCode { get; }

	public RoleException(string errorCode, string message)
		: base(message)
	{
		ErrorCode = errorCode;
	}

	public RoleException(string errorCode, string message, Exception innerException)
		: base(message, innerException)
	{
		ErrorCode = errorCode;
	}
}

