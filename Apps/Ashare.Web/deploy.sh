#!/bin/bash

PROJECT_ID="ashare-app"
REGION="me-central1"
SERVICE_NAME="ashare-web"
IMAGE_NAME="me-central1-docker.pkg.dev/$PROJECT_ID/ashare/ashare-web"

echo "🚀 نشر Ashare Web إلى Google Cloud Run..."
echo "========================================="

cd "$(dirname "$0")/../.."

echo "📦 بناء Docker image..."
docker build -t $IMAGE_NAME:latest -f Apps/Ashare.Web/Dockerfile .

if [ $? -ne 0 ]; then
    echo "❌ فشل بناء Docker image"
    exit 1
fi

echo "📤 رفع الصورة إلى Google Artifact Registry..."
docker push $IMAGE_NAME:latest

if [ $? -ne 0 ]; then
    echo "❌ فشل رفع الصورة"
    exit 1
fi

echo "🌐 نشر إلى Cloud Run..."
gcloud run deploy $SERVICE_NAME \
    --image $IMAGE_NAME:latest \
    --region $REGION \
    --platform managed \
    --allow-unauthenticated \
    --port 8080 \
    --memory 512Mi \
    --cpu 1 \
    --min-instances 0 \
    --max-instances 10

if [ $? -eq 0 ]; then
    echo ""
    echo "✅ تم النشر بنجاح!"
    echo "🔗 رابط التطبيق:"
    gcloud run services describe $SERVICE_NAME --region $REGION --format='value(status.url)'
else
    echo "❌ فشل النشر"
    exit 1
fi
