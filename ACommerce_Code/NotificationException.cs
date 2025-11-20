using ACommerce.Notifications.Abstractions.Enums;

namespace ACommerce.Notifications.Abstractions.Exceptions;

/// <summary>
/// ??????? ??? ?????????
/// </summary>
public class NotificationException : Exception
{
	public string ErrorCode { get; }
	public NotificationChannel? Channel { get; }

	public NotificationException(string errorCode, string message)
		: base(message)
	{
		ErrorCode = errorCode;
	}

	public NotificationException(
		string errorCode,
		string message,
		NotificationChannel channel)
		: base(message)
	{
		ErrorCode = errorCode;
		Channel = channel;
	}

	public NotificationException(
		string errorCode,
		string message,
		Exception innerException)
		: base(message, innerException)
	{
		ErrorCode = errorCode;
	}

	public NotificationException(
		string errorCode,
		string message,
		Exception innerException,
		NotificationChannel channel)
		: base(message, innerException)
	{
		ErrorCode = errorCode;
		Channel = channel;
	}
}

/// <summary>
/// ??????? ??? ??? ??????? ??? ???? ?????
/// </summary>
public class ChannelDeliveryException : NotificationException
{
	public ChannelDeliveryException(
		NotificationChannel channel,
		string message)
		: base("CHANNEL_DELIVERY_FAILED", message, channel)
	{
	}

	public ChannelDeliveryException(
		NotificationChannel channel,
		string message,
		Exception innerException)
		: base("CHANNEL_DELIVERY_FAILED", message, innerException, channel)
	{ }
}

