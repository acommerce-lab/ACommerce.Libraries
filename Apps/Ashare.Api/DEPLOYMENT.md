# دليل نشر Ashare API على Google Cloud Run

## المتطلبات الأساسية

1. **Google Cloud SDK** مثبت ومُعد
2. **Docker** مثبت (للبناء المحلي فقط)
3. **مشروع Google Cloud** مع تفعيل الخدمات التالية:
   - Cloud Run API
   - Cloud Build API
   - Artifact Registry API

## معلومات Artifact Registry

- **Repository Name**: `ashare-repo`
- **Type**: Docker
- **Region**: `me-central1` (Doha)

---

## الطريقة الأولى: النشر اليدوي (باستخدام Docker و gcloud)

### الخطوة 1: تسجيل الدخول وإعداد المشروع

```bash
# تسجيل الدخول
gcloud auth login

# تعيين المشروع
gcloud config set project YOUR_PROJECT_ID

# تفعيل Artifact Registry
gcloud auth configure-docker me-central1-docker.pkg.dev
```

### الخطوة 2: بناء صورة Docker

```bash
# من المجلد الرئيسي للمشروع
docker build -t me-central1-docker.pkg.dev/YOUR_PROJECT_ID/ashare-repo/ashare-api:latest \
  -f Apps/Ashare.Api/Dockerfile .
```

### الخطوة 3: رفع الصورة إلى Artifact Registry

```bash
docker push me-central1-docker.pkg.dev/YOUR_PROJECT_ID/ashare-repo/ashare-api:latest
```

### الخطوة 4: نشر على Cloud Run

```bash
gcloud run deploy ashare-api \
  --image me-central1-docker.pkg.dev/YOUR_PROJECT_ID/ashare-repo/ashare-api:latest \
  --region me-central1 \
  --platform managed \
  --allow-unauthenticated \
  --port 8080 \
  --memory 1Gi \
  --cpu 1 \
  --min-instances 0 \
  --max-instances 10 \
  --set-env-vars "SKIP_SEEDING=true,ASPNETCORE_ENVIRONMENT=Production"
```

---

## الطريقة الثانية: النشر التلقائي (باستخدام Cloud Build)

### الخطوة 1: تشغيل Cloud Build

```bash
# من المجلد الرئيسي للمشروع
gcloud builds submit --config Apps/Ashare.Api/cloudbuild.yaml .
```

هذا الأمر سيقوم تلقائياً بـ:
1. بناء صورة Docker
2. رفعها إلى Artifact Registry
3. نشرها على Cloud Run

---

## إضافة متغيرات البيئة والأسرار

### متغيرات البيئة المطلوبة

| المتغير | الوصف | مثال |
|---------|-------|------|
| `SKIP_SEEDING` | تخطي البذر الأولي | `true` |
| `ASPNETCORE_ENVIRONMENT` | بيئة التشغيل | `Production` |
| `HostSettings__BaseUrl` | رابط API | `https://ashare-api-xxxxx.me-central1.run.app` |
| `HostSettings__WebBaseUrl` | رابط تطبيق الويب | `https://ashare-web-xxxxx.me-central1.run.app` |

### إضافة Secret لقاعدة البيانات

```bash
# إنشاء Secret في Secret Manager
gcloud secrets create GOOGLE_SQL_CONNECTION_STRING \
  --data-file=- <<< "Server=YOUR_SERVER;Database=ashare;User Id=YOUR_USER;Password=YOUR_PASSWORD;"

# إعطاء صلاحية للـ Cloud Run service account
gcloud secrets add-iam-policy-binding GOOGLE_SQL_CONNECTION_STRING \
  --member="serviceAccount:YOUR_PROJECT_NUMBER-compute@developer.gserviceaccount.com" \
  --role="roles/secretmanager.secretAccessor"

# تحديث Cloud Run لاستخدام Secret
gcloud run services update ashare-api \
  --region me-central1 \
  --set-secrets "GOOGLE_SQL_CONNECTION_STRING=GOOGLE_SQL_CONNECTION_STRING:latest"
```

### تحديث متغيرات البيئة بعد النشر

```bash
gcloud run services update ashare-api \
  --region me-central1 \
  --set-env-vars "SKIP_SEEDING=true,ASPNETCORE_ENVIRONMENT=Production,HostSettings__BaseUrl=https://YOUR_CLOUD_RUN_URL"
```

---

## التحقق من النشر

### فحص الصحة (Health Check)

```bash
# الحصول على رابط الخدمة
SERVICE_URL=$(gcloud run services describe ashare-api --region me-central1 --format 'value(status.url)')

# فحص الصحة
curl $SERVICE_URL/health
```

### فحص الجاهزية (Readiness Check)

```bash
curl $SERVICE_URL/ready
```

### عرض Swagger UI

افتح في المتصفح:
```
https://YOUR_CLOUD_RUN_URL/
```

---

## إعداد CI/CD مع GitHub

### ربط GitHub مع Cloud Build

1. اذهب إلى [Cloud Build Triggers](https://console.cloud.google.com/cloud-build/triggers)
2. اضغط "Create Trigger"
3. اختر "Connect Repository" واربط مستودع GitHub
4. أعد الـ Trigger:
   - **Name**: `deploy-ashare-api`
   - **Event**: Push to a branch
   - **Branch**: `^main$` أو `^master$`
   - **Build Configuration**: Cloud Build configuration file
   - **Path**: `Apps/Ashare.Api/cloudbuild.yaml`

الآن سيتم النشر التلقائي عند كل push إلى الفرع الرئيسي.

---

## استكشاف الأخطاء

### مشاكل شائعة

1. **خطأ في الاتصال بقاعدة البيانات**
   - تأكد من أن `GOOGLE_SQL_CONNECTION_STRING` معد بشكل صحيح
   - تأكد من فتح الوصول من Cloud Run إلى Cloud SQL

2. **الخدمة لا تستجيب**
   - تحقق من logs: `gcloud run logs read ashare-api --region me-central1`
   - تأكد من أن PORT = 8080

3. **مشاكل الذاكرة**
   - زيادة الذاكرة: `--memory 2Gi`

### عرض السجلات

```bash
# آخر 100 سجل
gcloud run logs read ashare-api --region me-central1 --limit 100

# متابعة السجلات مباشرة
gcloud run logs tail ashare-api --region me-central1
```

---

## المنافذ والإعدادات

| الإعداد | القيمة |
|---------|--------|
| Port | 8080 |
| Health Check | `/health` |
| Readiness Check | `/ready` |
| Swagger UI | `/` |
| Min Instances | 0 |
| Max Instances | 10 |
| Memory | 1Gi |
| CPU | 1 |
