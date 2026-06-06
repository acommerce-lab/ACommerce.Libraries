# ACommerce.AppConfig — اتفاقية تسمية Feature Flags

> **القاعدة:** اسم العلامة يخبر بمفرده ما يحدث عند تعطيلها.

## ثلاثة أنماط

| النمط | الدلالة | عند `Enabled = true` | عند `Enabled = false` |
|---|---|---|---|
| `<area>.enabled` | **توفّر** (Availability) | الميزة موجودة ومُفعّلة كاملاً. | الميزة **غير موجودة** — كل ظهورها في الواجهة يختفي، والمستخدم لا يستطيع الوصول لها (لا توجد بدائل). |
| `<area>.<step>_required` | **سياسة/اشتراط** (Policy) | الخطوة **إلزامية** على المستخدم. | الخطوة **تُتجاوز** والمستخدم يمر مباشرة بدونها. |
| `<area>.force_<x>` | **إنفاذ** (Enforcement) | إنفاذ القاعدة فعّال. | الإنفاذ معطّل — يُسمح بالمتابعة حتى لو خالف الشرط. |

## أمثلة من Ashare

```text
payments.enabled            availability  → معطّلة: لا يستطيع المستخدم إتمام أي دفعة
chat.enabled                availability  → معطّلة: الشات يختفي تماماً، لا يوجد تواصل
booking.enabled             availability  → معطّلة: زر/صفحة الحجز تختفي
subscriptions.enabled       availability  → معطّلة: صفحات الاشتراك تختفي + يُتخطّى فحص الاشتراك في إنشاء العرض
complaints.enabled          availability  → معطّلة: الشكاوى تختفي
auth.nafath                 availability  → معطّلة: تسجيل الدخول بنفاذ يختفي
auth.guest_mode             availability  → معطّلة: زر «متابعة كزائر» يختفي
listing.disclaimer.show     availability  → معطّلة: قسم إخلاء المسؤولية يختفي

booking.deposit_required    policy        → معطّلة: الحجز يُنشأ مباشرة بدون عربون
profile.photo_required      policy        → معطّلة: الصورة اختيارية عند التسجيل

version_check.enabled       availability  → معطّلة: لا يحدث فحص إصدار أصلاً
version_check.force_update  enforcement   → معطّلة: حتى لو الإصدار قديم لا يُحجَب التطبيق
```

## القرار: عند تعطيل ميزة هل المستخدم يُسمح له بالمتابعة؟

اقرأ اسم العلامة:
- `*.enabled` معطّلة → **المستخدم مُمنع** من سلوكها (لأنها غير موجودة).
- `*.required` معطّلة → **المستخدم يمر** (لأن الخطوة لم تعد مطلوبة).
- `*.force_*` معطّلة → **المستخدم يمر** (لأن الإنفاذ توقّف).

## أمثلة سيناريوهات مُركّبة

### الدفع + العربون

| `payments.enabled` | `booking.deposit_required` | السلوك |
|---|---|---|
| true | true | التدفّق العادي — يطلب عربون، يفتح بوابة الدفع، ينتظر التأكيد |
| true | false | **يتم تخطّي** خطوة العربون — الحجز يُنشأ مباشرة بحالة Confirmed |
| false | true | **محظور** — يظهر للمستخدم «الدفع غير متاح والعربون مطلوب» |
| false | false | **يمر** — الحجز يُنشأ مباشرة بدون أي محاولة دفع |

### الإصدار

| `version_check.enabled` | `version_check.force_update` | السلوك |
|---|---|---|
| true | true | الفحص يحدث؛ لو الإصدار قديم يُحجَب التطبيق |
| true | false | الفحص يحدث؛ يُعرض **تنبيه** فقط، لا حجب |
| false | * | لا فحص أصلاً، لا تنبيه ولا حجب |

## كيف تضيف علامة جديدة؟

1. **اختر النمط الصحيح من الاسم** — هذا يحدّد المتوقّع.
2. أضفها في `AshareSeedDataService.SeedFeatureFlagsAsync` مع `Description` يشرح ما يحدث عند التعطيل.
3. غلّف كل ظهورها في الواجهة بـ `<FeatureGate Feature="...">`.
4. للسلوك على مستوى الصفحة استخدم `FeatureGuard.RequireOrRedirect("...")` أو `FeatureGuard.IsEnabled("...")` في `OnInitializedAsync`.
5. تحقّق من **العزل**: لا تضع `Features.IsEnabled("X") && Features.IsEnabled("Y")` إلا إذا كنت **تقصد** اعتمادية بينهما (وثّقها صراحةً).

## ما لا يُغلَّف بـ Feature Flag

- **بيانات قابلة للحذف الناعم في DB** (مثل خطط الاشتراك `SubscriptionPlan.IsActive`، فئات `ProductCategory.IsActive`) — أدِرها من لوحة الإدارة مباشرة.
- **اختيار مزوّد** (Noon vs Moyasar، Meta vs Google) — أدِره من `appsettings.json` أو من خدمة اختيار الـ provider.
- **محتوى ديناميكي** (نصوص، ألوان، إعدادات رقمية) — استخدم `UiString` / `ThemeToken` / `AppConfigEntry` بدلاً من `FeatureFlag`.
