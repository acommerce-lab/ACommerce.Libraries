namespace ACommerce.MarketplaceApi.Services;

/// <summary>
/// Mock Authentication Service للتجربة
/// في الإنتاج، استخدم ACommerce.Authentication.JWT
/// </summary>
public class MockAuthService
{
	private readonly Dictionary<string, User> _users = new();

	public MockAuthService()
	{
		// بيانات تجريبية
		SeedUsers();
	}

	private void SeedUsers()
	{
		// عميل تجريبي
		_users["customer@example.com"] = new User
		{
			Id = "customer-001",
			Email = "customer@example.com",
			Password = "123456", // في الإنتاج: hashed!
			FullName = "أحمد محمد",
			Role = "Customer"
		};

		// بائع تجريبي
		_users["vendor@example.com"] = new User
		{
			Id = "vendor-001",
			Email = "vendor@example.com",
			Password = "123456",
			FullName = "متجر الإلكترونيات",
			Role = "Vendor"
		};

		// أدمن تجريبي
		_users["admin@example.com"] = new User
		{
			Id = "admin-001",
			Email = "admin@example.com",
			Password = "123456",
			FullName = "المدير",
			Role = "Admin"
		};
	}

	public LoginResult Login(string email, string password)
	{
		if (!_users.TryGetValue(email, out var user))
			return new LoginResult { Success = false, Message = "البريد الإلكتروني غير موجود" };

		if (user.Password != password)
			return new LoginResult { Success = false, Message = "كلمة المرور خاطئة" };

		return new LoginResult
		{
			Success = true,
			Token = $"mock-token-{user.Id}",
			User = user,
			Message = "تم تسجيل الدخول بنجاح"
		};
	}

	public RegisterResult Register(string email, string password, string fullName, string role = "Customer")
	{
		if (_users.ContainsKey(email))
			return new RegisterResult { Success = false, Message = "البريد الإلكتروني مستخدم بالفعل" };

		var user = new User
		{
			Id = Guid.NewGuid().ToString(),
			Email = email,
			Password = password, // في الإنتاج: hash!
			FullName = fullName,
			Role = role
		};

		_users[email] = user;

		return new RegisterResult
		{
			Success = true,
			User = user,
			Message = "تم إنشاء الحساب بنجاح"
		};
	}

	public User? GetUserByToken(string token)
	{
		var userId = token.Replace("mock-token-", "");
		return _users.Values.FirstOrDefault(u => u.Id == userId);
	}

	public User? GetUserById(string userId)
	{
		return _users.Values.FirstOrDefault(u => u.Id == userId);
	}

	public IEnumerable<User> GetAllUsers() => _users.Values;
}

public class User
{
	public required string Id { get; set; }
	public required string Email { get; set; }
	public required string Password { get; set; }
	public required string FullName { get; set; }
	public required string Role { get; set; }
}

public class LoginResult
{
	public bool Success { get; set; }
	public string? Token { get; set; }
	public User? User { get; set; }
	public required string Message { get; set; }
}

public class RegisterResult
{
	public bool Success { get; set; }
	public User? User { get; set; }
	public required string Message { get; set; }
}
