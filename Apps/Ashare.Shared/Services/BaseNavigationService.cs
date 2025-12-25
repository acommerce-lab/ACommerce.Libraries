using Microsoft.AspNetCore.Components;
using ACommerce.Templates.Customer.Services;

namespace Ashare.Shared.Services;

/// <summary>
/// خدمة التنقل الأساسية مع إدارة السجل
/// ترث منها خدمات التنقل الخاصة بكل منصة (MAUI/Web)
/// </summary>
public abstract class BaseNavigationService : IAppNavigationService
{
    protected readonly NavigationManager NavigationManager;
    private readonly Stack<string> _history = new();

    protected BaseNavigationService(NavigationManager navigationManager)
    {
        NavigationManager = navigationManager;
    }

    /// <summary>
    /// المسار الحالي
    /// </summary>
    public string CurrentUri => NavigationManager.Uri;

    /// <summary>
    /// عدد الصفحات في السجل
    /// </summary>
    public int HistoryCount => _history.Count;

    /// <summary>
    /// التنقل إلى مسار جديد مع حفظ المسار الحالي في السجل
    /// </summary>
    public virtual void NavigateTo(string uri, bool forceLoad = false)
    {
        _history.Push(NavigationManager.Uri);
        NavigationManager.NavigateTo(uri, forceLoad);
    }

    /// <summary>
    /// العودة للصفحة السابقة
    /// </summary>
    public virtual void NavigateBack()
    {
        if (_history.Count > 0)
        {
            var previousUri = _history.Pop();
            NavigationManager.NavigateTo(previousUri);
        }
        else
        {
            NavigationManager.NavigateTo("/");
        }
    }

    /// <summary>
    /// التنقل مع مسح السجل (للخروج)
    /// </summary>
    public virtual void NavigateToAndClearHistory(string uri)
    {
        _history.Clear();
        NavigationManager.NavigateTo(uri, forceLoad: true);
    }

    /// <summary>
    /// مسح السجل يدوياً
    /// </summary>
    public void ClearHistory()
    {
        _history.Clear();
    }

    /// <summary>
    /// فتح الموقع في تطبيق الخرائط - تنفذ حسب المنصة
    /// </summary>
    public abstract Task OpenMapAsync(double latitude, double longitude, string? label = null);

    /// <summary>
    /// فتح رابط خارجي - تنفذ حسب المنصة
    /// </summary>
    public abstract Task OpenExternalAsync(string url);
}
