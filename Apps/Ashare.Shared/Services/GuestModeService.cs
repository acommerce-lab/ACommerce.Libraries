namespace Ashare.Shared.Services;

/// <summary>
/// خدمة إدارة وضع الضيف
/// </summary>
public class GuestModeService
{
    private bool _isGuestMode;

    /// <summary>
    /// هل المستخدم في وضع الضيف
    /// </summary>
    public bool IsGuestMode => _isGuestMode;

    /// <summary>
    /// تفعيل وضع الضيف
    /// </summary>
    public void EnableGuestMode()
    {
        _isGuestMode = true;
        OnGuestModeChanged?.Invoke();
    }

    /// <summary>
    /// إلغاء وضع الضيف
    /// </summary>
    public void DisableGuestMode()
    {
        _isGuestMode = false;
        OnGuestModeChanged?.Invoke();
    }

    /// <summary>
    /// حدث تغيير وضع الضيف
    /// </summary>
    public event Action? OnGuestModeChanged;
}
