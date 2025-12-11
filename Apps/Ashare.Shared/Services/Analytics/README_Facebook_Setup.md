# Facebook App Events Setup for MAUI

## المتطلبات

1. **Facebook App ID**: `1775307086509416` (تم توفيره)
2. **Access Token** (اختياري): للتحقق من الأحداث

## الحصول على Access Token (اختياري)

1. اذهب إلى https://developers.facebook.com/tools/explorer
2. اختر التطبيق
3. أنشئ Access Token مع صلاحية `ads_management`
4. أو استخدم App Access Token: `{App ID}|{App Secret}`

---

## إعداد Android

### 1. إضافة في `Platforms/Android/AndroidManifest.xml`:

```xml
<application>
    <!-- Facebook App ID -->
    <meta-data 
        android:name="com.facebook.sdk.ApplicationId" 
        android:value="@string/facebook_app_id"/>
    
    <!-- Facebook Client Token -->
    <meta-data 
        android:name="com.facebook.sdk.ClientToken" 
        android:value="@string/facebook_client_token"/>
    
    <!-- Enable Auto App Events Logging -->
    <meta-data 
        android:name="com.facebook.sdk.AutoLogAppEventsEnabled"
        android:value="true"/>
    
    <!-- Enable Advertiser ID Collection -->
    <meta-data 
        android:name="com.facebook.sdk.AdvertiserIDCollectionEnabled"
        android:value="true"/>
</application>
```

### 2. إضافة في `Platforms/Android/Resources/values/strings.xml`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<resources>
    <string name="facebook_app_id">1775307086509416</string>
    <string name="facebook_client_token">YOUR_CLIENT_TOKEN_HERE</string>
</resources>
```

---

## إعداد iOS

### إضافة في `Platforms/iOS/Info.plist`:

```xml
<key>CFBundleURLTypes</key>
<array>
    <dict>
        <key>CFBundleURLSchemes</key>
        <array>
            <string>fb1775307086509416</string>
        </array>
    </dict>
</array>

<key>FacebookAppID</key>
<string>1775307086509416</string>

<key>FacebookClientToken</key>
<string>YOUR_CLIENT_TOKEN_HERE</string>

<key>FacebookDisplayName</key>
<string>Ashare</string>

<!-- iOS 14+ App Tracking Transparency -->
<key>NSUserTrackingUsageDescription</key>
<string>نستخدم هذا لتحسين تجربتك وعرض إعلانات مخصصة</string>
```

---

## الاستخدام في MauiProgram.cs

```csharp
using Ashare.Shared.Services.Analytics;

public static MauiApp CreateMauiApp()
{
    var builder = MauiApp.CreateBuilder();
    
    // إضافة Facebook Analytics
    builder.Services.AddFacebookMobileAnalytics(
        appId: "1775307086509416",
        accessToken: null, // اختياري - أضف Access Token إذا كان متاحاً
        debugMode: true // اجعلها false في الإنتاج
    );

    return builder.Build();
}
```

---

## تتبع الأحداث

### في أي صفحة أو خدمة:

```csharp
@inject MetaMobileAnalyticsProvider FacebookAnalytics

// تتبع عرض محتوى
await FacebookAnalytics.TrackContentViewAsync(new ContentViewEvent
{
    ContentId = "listing_123",
    ContentName = "مساحة مكتبية",
    ContentType = "listing",
    Value = 500,
    Currency = "SAR"
});

// تتبع بحث
await FacebookAnalytics.TrackSearchAsync("مكتب في الرياض");

// تتبع إضافة للسلة
await FacebookAnalytics.TrackAddToCartAsync(new ContentViewEvent
{
    ContentId = "listing_123",
    ContentName = "مساحة مكتبية",
    Value = 500,
    Currency = "SAR"
});

// تتبع شراء
await FacebookAnalytics.TrackPurchaseAsync(new PurchaseEvent
{
    TransactionId = "order_456",
    Value = 500,
    Currency = "SAR",
    Items = new List<PurchaseItem>
    {
        new() { ItemId = "listing_123", ItemName = "مساحة مكتبية", Price = 500 }
    }
});

// تتبع تسجيل
await FacebookAnalytics.TrackRegistrationAsync("nafath");

// تتبع تواصل
await FacebookAnalytics.TrackContactAsync();

// تتبع بدء الدفع
await FacebookAnalytics.TrackInitiateCheckoutAsync(500, "SAR", 1);

// تتبع إضافة معلومات الدفع
await FacebookAnalytics.TrackAddPaymentInfoAsync(success: true);

// تتبع اشتراك
await FacebookAnalytics.TrackSubscribeAsync();

// تتبع تقييم
await FacebookAnalytics.TrackRateAsync(rating: 4.5m, maxRating: 5);
```

---

## الأحداث المدعومة

| الحدث | الدالة |
|-------|--------|
| Contact | `TrackContactAsync()` |
| Search | `TrackSearchAsync(term, params)` |
| Complete Tutorial | `TrackCompleteTutorialAsync(success, contentId)` |
| Complete Registration | `TrackRegistrationAsync(method)` |
| View Content | `TrackContentViewAsync(content)` |
| Achieve Level | `TrackAchieveLevelAsync(level)` |
| Subscribe | `TrackSubscribeAsync()` |
| Add to Cart | `TrackAddToCartAsync(content)` |
| Customize Product | `TrackCustomizeProductAsync()` |
| Unlock Achievement | `TrackUnlockAchievementAsync(description)` |
| Find Location | `TrackFindLocationAsync()` |
| Spend Credits | `TrackSpendCreditsAsync(value, currency)` |
| Donate | `TrackDonateAsync()` |
| Add to Wishlist | `TrackAddToWishlistAsync(content)` |
| Initiate Checkout | `TrackInitiateCheckoutAsync(value, currency, numItems)` |
| Start Trial | `TrackStartTrialAsync()` |
| Schedule | `TrackScheduleAsync()` |
| Submit Application | `TrackSubmitApplicationAsync()` |
| Rate | `TrackRateAsync(rating, maxRating)` |
| Purchase | `TrackPurchaseAsync(purchase)` |
| Add Payment Info | `TrackAddPaymentInfoAsync(success)` |

---

## التحقق من الإعداد

1. اذهب إلى https://business.facebook.com/events_manager
2. اختر التطبيق
3. استخدم "Test Events" لمشاهدة الأحداث بالوقت الفعلي
4. الأحداث تظهر خلال 24-48 ساعة في التقارير

---

## ملاحظات مهمة

- **iOS 14+**: يجب طلب إذن ATT (App Tracking Transparency) قبل جمع IDFA
- **Android**: تأكد من إضافة Google Play Services للحصول على Advertising ID
- **Debug Mode**: فعّله أثناء التطوير لمشاهدة الأحداث في Console
