# ACommerce.Notifications.Channels.Firebase

Firebase Cloud Messaging (FCM) push notification channel for mobile and web.

## Overview

Complete Firebase integration for push notifications to Android, iOS, and web applications. Supports both legacy HTTP v1 and the latest Firebase Admin SDK.

## Key Features

✅ **Multi-Platform** - Android, iOS, Web  
✅ **Topic Messaging** - Broadcast to topics  
✅ **Data Messages** - Custom data payloads  
✅ **Notification Messages** - System notifications  
✅ **Rich Notifications** - Images, actions, sounds  
✅ **Badge Management** - iOS badge counts  
✅ **Analytics** - Delivery tracking  

## Configuration

### appsettings.json
```json
{
  "FirebaseSettings": {
    "ServiceAccountJsonPath": "firebase-adminsdk.json",
    "ProjectId": "ACommerce-app",
    "SenderId": "123456789",
    "DefaultIcon": "https://ACommerce.sa/icon.png",
    "DefaultSound": "default",
    "EnableAnalytics": true,
    "TimeToLive": 2419200
  }
}
```

### firebase-adminsdk.json

Download from Firebase Console → Project Settings → Service Accounts
```json
{
  "type": "service_account",
  "project_id": "ACommerce-app",
  "private_key_id": "...",
  "private_key": "-----BEGIN PRIVATE KEY-----\n...\n-----END PRIVATE KEY-----\n",
  "client_email": "firebase-adminsdk-xxxxx@ACommerce-app.iam.gserviceaccount.com",
  "client_id": "...",
  "auth_uri": "https://accounts.google.com/o/oauth2/auth",
  "token_uri": "https://oauth2.googleapis.com/token",
  "auth_provider_x509_cert_url": "https://www.googleapis.com/oauth2/v1/certs",
  "client_x509_cert_url": "..."
}
```

## Setup
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add Firebase Push Notifications
builder.Services.AddFirebasePushNotifications(builder.Configuration);

var app = builder.Build();
app.Run();
```

## Usage

### Simple Push Notification
```csharp
var notification = new Notification
{
    UserId = "user-123",
    Title = "New Message",
    Message = "You have a new message from John",
    Type = NotificationType.Info,
    Channels = new List<NotificationChannel> { NotificationChannel.Push },
    Data = new Dictionary<string, string>
    {
        ["DeviceToken"] = "fcm-device-token-here",
        ["Icon"] = "message",
        ["Sound"] = "notification.mp3",
        ["Badge"] = "5"
    }
};

var result = await _notificationService.SendAsync(notification);
```

### Rich Notification with Image
```csharp
var notification = new Notification
{
    UserId = "user-123",
    Title = "New Product",
    Message = "Check out our new product!",
    Channels = new List<NotificationChannel> { NotificationChannel.Push },
    Data = new Dictionary<string, string>
    {
        ["DeviceToken"] = "fcm-token",
        ["ImageUrl"] = "https://ACommerce.sa/products/image.jpg",
        ["ClickAction"] = "PRODUCT_DETAILS",
        ["ProductId"] = "prod-123"
    }
};

await _notificationService.SendAsync(notification);
```

### Topic Broadcast
```csharp
var notification = new Notification
{
    Title = "System Maintenance",
    Message = "The app will be down for maintenance tonight",
    Type = NotificationType.SystemAlert,
    Channels = new List<NotificationChannel> { NotificationChannel.Push },
    Data = new Dictionary<string, string>
    {
        ["Topic"] = "all_users",
        ["Priority"] = "high"
    }
};

await _notificationService.SendAsync(notification);
```

## Device Token Management

### Register Device Token
```csharp
[HttpPost("register-device")]
[Authorize]
public async Task<IActionResult> RegisterDevice([FromBody] RegisterDeviceRequest request)
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    
    await _deviceTokenService.RegisterAsync(new DeviceToken
    {
        UserId = userId,
        Token = request.Token,
        Platform = request.Platform, // iOS, Android, Web
        DeviceId = request.DeviceId,
        IsActive = true
    });
    
    return Ok();
}
```

### Subscribe to Topic
```csharp
[HttpPost("subscribe-topic")]
[Authorize]
public async Task<IActionResult> SubscribeTopic([FromBody] SubscribeTopicRequest request)
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var deviceTokens = await _deviceTokenService.GetByUserAsync(userId);
    
    await _firebaseService.SubscribeToTopicAsync(
        deviceTokens.Select(t => t.Token).ToList(),
        request.Topic);
    
    return Ok();
}
```

## Mobile Integration

### Android (Kotlin)

#### build.gradle
```gradle
dependencies {
    implementation platform('com.google.firebase:firebase-bom:32.7.0')
    implementation 'com.google.firebase:firebase-messaging'
}
```

#### FirebaseMessagingService
```kotlin
class MyFirebaseMessagingService : FirebaseMessagingService() {
    
    override fun onNewToken(token: String) {
        // Send token to backend
        sendTokenToServer(token)
    }
    
    override fun onMessageReceived(message: RemoteMessage) {
        // Handle notification
        message.notification?.let {
            showNotification(it.title, it.body, message.data)
        }
        
        // Handle data payload
        message.data.isNotEmpty().let {
            handleDataPayload(message.data)
        }
    }
    
    private fun sendTokenToServer(token: String) {
        val request = RegisterDeviceRequest(
            token = token,
            platform = "Android",
            deviceId = getDeviceId()
        )
        
        apiService.registerDevice(request)
    }
    
    private fun showNotification(title: String?, body: String?, data: Map<String, String>) {
        val intent = Intent(this, MainActivity::class.java).apply {
            flags = Intent.FLAG_ACTIVITY_NEW_TASK or Intent.FLAG_ACTIVITY_CLEAR_TASK
            putExtras(Bundle().apply {
                data.forEach { (key, value) -> putString(key, value) }
            })
        }
        
        val pendingIntent = PendingIntent.getActivity(
            this, 0, intent, PendingIntent.FLAG_IMMUTABLE
        )
        
        val notification = NotificationCompat.Builder(this, CHANNEL_ID)
            .setSmallIcon(R.drawable.ic_notification)
            .setContentTitle(title)
            .setContentText(body)
            .setPriority(NotificationCompat.PRIORITY_HIGH)
            .setContentIntent(pendingIntent)
            .setAutoCancel(true)
            .build()
        
        NotificationManagerCompat.from(this).notify(NOTIFICATION_ID, notification)
    }
}
```

### iOS (Swift)

#### AppDelegate.swift
```swift
import Firebase
import UserNotifications

@UIApplicationMain
class AppDelegate: UIResponder, UIApplicationDelegate {
    
    func application(_ application: UIApplication, 
                     didFinishLaunchingWithOptions launchOptions: [UIApplication.LaunchOptionsKey: Any]?) -> Bool {
        
        // Configure Firebase
        FirebaseApp.configure()
        
        // Request notification permissions
        UNUserNotificationCenter.current().requestAuthorization(options: [.alert, .badge, .sound]) { granted, error in
            if granted {
                DispatchQueue.main.async {
                    application.registerForRemoteNotifications()
                }
            }
        }
        
        UNUserNotificationCenter.current().delegate = self
        Messaging.messaging().delegate = self
        
        return true
    }
    
    func application(_ application: UIApplication, 
                     didRegisterForRemoteNotificationsWithDeviceToken deviceToken: Data) {
        Messaging.messaging().apnsToken = deviceToken
    }
}

extension AppDelegate: MessagingDelegate {
    func messaging(_ messaging: Messaging, didReceiveRegistrationToken fcmToken: String?) {
        guard let token = fcmToken else { return }
        
        // Send token to backend
        sendTokenToServer(token: token)
    }
    
    private func sendTokenToServer(token: String) {
        let request = RegisterDeviceRequest(
            token: token,
            platform: "iOS",
            deviceId: UIDevice.current.identifierForVendor?.uuidString ?? ""
        )
        
        APIService.shared.registerDevice(request)
    }
}

extension AppDelegate: UNUserNotificationCenterDelegate {
    func userNotificationCenter(_ center: UNUserNotificationCenter,
                                willPresent notification: UNNotification,
                                withCompletionHandler completionHandler: @escaping (UNNotificationPresentationOptions) -> Void) {
        completionHandler([.banner, .badge, .sound])
    }
    
    func userNotificationCenter(_ center: UNUserNotificationCenter,
                                didReceive response: UNNotificationResponse,
                                withCompletionHandler completionHandler: @escaping () -> Void) {
        let userInfo = response.notification.request.content.userInfo
        
        // Handle notification tap
        handleNotificationTap(userInfo: userInfo)
        
        completionHandler()
    }
}
```

### Web (JavaScript)

#### firebase-messaging-sw.js
```javascript
importScripts('https://www.gstatic.com/firebasejs/10.7.1/firebase-app-compat.js');
importScripts('https://www.gstatic.com/firebasejs/10.7.1/firebase-messaging-compat.js');

firebase.initializeApp({
  apiKey: "...",
  authDomain: "ACommerce-app.firebaseapp.com",
  projectId: "ACommerce-app",
  storageBucket: "ACommerce-app.appspot.com",
  messagingSenderId: "123456789",
  appId: "..."
});

const messaging = firebase.messaging();

messaging.onBackgroundMessage((payload) => {
  console.log('Received background message:', payload);
  
  const notificationTitle = payload.notification.title;
  const notificationOptions = {
    body: payload.notification.body,
    icon: payload.notification.icon || '/icon.png',
    badge: '/badge.png',
    data: payload.data
  };
  
  self.registration.showNotification(notificationTitle, notificationOptions);
});
```

#### App.js
```javascript
import { initializeApp } from 'firebase/app';
import { getMessaging, getToken, onMessage } from 'firebase/messaging';

const firebaseConfig = {
  apiKey: "...",
  authDomain: "ACommerce-app.firebaseapp.com",
  projectId: "ACommerce-app",
  storageBucket: "ACommerce-app.appspot.com",
  messagingSenderId: "123456789",
  appId: "..."
};

const app = initializeApp(firebaseConfig);
const messaging = getMessaging(app);

// Request permission and get token
export const requestPermission = async () => {
  try {
    const permission = await Notification.requestPermission();
    
    if (permission === 'granted') {
      const token = await getToken(messaging, {
        vapidKey: 'your-vapid-key'
      });
      
      // Send token to backend
      await registerDevice(token);
      
      return token;
    }
  } catch (error) {
    console.error('Error getting permission:', error);
  }
};

// Handle foreground messages
onMessage(messaging, (payload) => {
  console.log('Received foreground message:', payload);
  
  // Show notification
  showNotification(payload.notification.title, payload.notification.body);
});

const registerDevice = async (token) => {
  await fetch('/api/notifications/register-device', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${accessToken}`
    },
    body: JSON.stringify({
      token,
      platform: 'Web',
      deviceId: getDeviceId()
    })
  });
};
```

## Testing

### Test Notification
```csharp
[HttpPost("test-push")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> TestPush([FromBody] TestPushRequest request)
{
    var notification = new Notification
    {
        UserId = request.UserId,
        Title = "Test Notification",
        Message = "This is a test push notification",
        Channels = new List<NotificationChannel> { NotificationChannel.Push },
        Data = new Dictionary<string, string>
        {
            ["DeviceToken"] = request.DeviceToken,
            ["TestMode"] = "true"
        }
    };
    
    var result = await _notificationService.SendAsync(notification);
    
    return Ok(result);
}
```

## Installation
```bash
dotnet add package ACommerce.Notifications.Channels.Firebase
dotnet add package FirebaseAdmin
```

## Dependencies

- ACommerce.Notifications.Abstractions
- FirebaseAdmin (2.4.0)

## License

MIT