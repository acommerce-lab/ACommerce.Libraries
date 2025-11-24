namespace ACommerce.Payments.Abstractions.Enums;

/// <summary>
/// حالة الدفع
/// </summary>
public enum PaymentStatus
{
	Pending = 1,
	Processing = 2,
	Completed = 3,
	Failed = 4,
	Cancelled = 5,
	Refunded = 6,
	PartiallyRefunded = 7
}

/// <summary>
/// طريقة الدفع
/// </summary>
public enum PaymentMethod
{
	CreditCard = 1,
	DebitCard = 2,
	BankTransfer = 3,
	Wallet = 4,
	CashOnDelivery = 5,
	ApplePay = 6,
	GooglePay = 7,
	Tabby = 8,
	Tamara = 9
}
