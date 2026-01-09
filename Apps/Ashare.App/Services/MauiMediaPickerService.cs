using ACommerce.Templates.Customer.Services;

namespace Ashare.App.Services;

/// <summary>
/// خدمة اختيار الوسائط باستخدام MAUI MediaPicker
/// تدعم الكاميرا والمعرض على Android و iOS
/// </summary>
public class MauiMediaPickerService : IMediaPickerService
{
    public bool IsCameraAvailable => MediaPicker.Default.IsCaptureSupported;

    public async Task<MediaPickResult?> CapturePhotoAsync()
    {
        try
        {
            // التحقق من الصلاحيات أولاً
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Camera>();
                if (status != PermissionStatus.Granted)
                {
                    Console.WriteLine("[MauiMediaPicker] Camera permission denied");
                    await ShowErrorAlertAsync("صلاحية الكاميرا مطلوبة",
                        "يرجى السماح للتطبيق باستخدام الكاميرا من إعدادات الجهاز");
                    return null;
                }
            }

            var photo = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions
            {
                Title = "التقاط صورة"
            });

            if (photo == null)
                return null;

            return new MediaPickResult
            {
                FilePath = photo.FullPath,
                FileName = photo.FileName,
                ContentType = photo.ContentType,
                OpenReadAsync = () => photo.OpenReadAsync()
            };
        }
        catch (FeatureNotSupportedException)
        {
            Console.WriteLine("[MauiMediaPicker] Camera not supported on this device");
            await ShowErrorAlertAsync("الكاميرا غير متاحة",
                "هذا الجهاز لا يدعم التقاط الصور");
            return null;
        }
        catch (PermissionException)
        {
            Console.WriteLine("[MauiMediaPicker] Camera permission not granted");
            await ShowErrorAlertAsync("صلاحية الكاميرا مطلوبة",
                "يرجى السماح للتطبيق باستخدام الكاميرا من إعدادات الجهاز");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MauiMediaPicker] Error capturing photo: {ex.Message}");
            await ShowErrorAlertAsync("خطأ في الكاميرا",
                "حدث خطأ أثناء فتح الكاميرا. يرجى المحاولة مرة أخرى");
            return null;
        }
    }

    private static async Task ShowErrorAlertAsync(string title, string message)
    {
        try
        {
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert(title, message, "حسناً");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MauiMediaPicker] Error showing alert: {ex.Message}");
        }
    }

    public async Task<MediaPickResult?> PickPhotoAsync()
    {
        try
        {
            // التحقق من صلاحية قراءة الصور
            var status = await Permissions.CheckStatusAsync<Permissions.Photos>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Photos>();
                if (status != PermissionStatus.Granted)
                {
                    Console.WriteLine("[MauiMediaPicker] Photos permission denied");
                    await ShowErrorAlertAsync("صلاحية الصور مطلوبة",
                        "يرجى السماح للتطبيق بالوصول إلى الصور من إعدادات الجهاز");
                    return null;
                }
            }

            var photo = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "اختر صورة"
            });

            if (photo == null)
                return null;

            return new MediaPickResult
            {
                FilePath = photo.FullPath,
                FileName = photo.FileName,
                ContentType = photo.ContentType,
                OpenReadAsync = () => photo.OpenReadAsync()
            };
        }
        catch (PermissionException)
        {
            Console.WriteLine("[MauiMediaPicker] Photos permission not granted");
            await ShowErrorAlertAsync("صلاحية الصور مطلوبة",
                "يرجى السماح للتطبيق بالوصول إلى الصور من إعدادات الجهاز");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MauiMediaPicker] Error picking photo: {ex.Message}");
            await ShowErrorAlertAsync("خطأ في اختيار الصورة",
                "حدث خطأ أثناء اختيار الصورة. يرجى المحاولة مرة أخرى");
            return null;
        }
    }

    public async Task<List<MediaPickResult>> PickPhotosAsync(int maxCount = 10)
    {
        var results = new List<MediaPickResult>();

        try
        {
#if ANDROID
            // التحقق من صلاحية قراءة الصور
            var status = await Permissions.CheckStatusAsync<Permissions.Photos>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Photos>();
                if (status != PermissionStatus.Granted)
                {
                    Console.WriteLine("[MauiMediaPicker] Photos permission denied");
                    await ShowErrorAlertAsync("صلاحية الصور مطلوبة",
                        "يرجى السماح للتطبيق بالوصول إلى الصور من إعدادات الجهاز");
                    return results;
                }
            }

            // على Android، نستخدم FilePicker لاختيار صور متعددة
            var options = new PickOptions
            {
                PickerTitle = "اختر صور (حتى " + maxCount + " صور)",
                FileTypes = FilePickerFileType.Images
            };

            var files = await FilePicker.Default.PickMultipleAsync(options);
            if (files != null)
            {
                foreach (var file in files.Take(maxCount))
                {
                    results.Add(new MediaPickResult
                    {
                        FilePath = file.FullPath,
                        FileName = file.FileName,
                        ContentType = file.ContentType,
                        OpenReadAsync = () => file.OpenReadAsync()
                    });
                }
            }
#else
            // على iOS ومنصات أخرى، نختار صورة واحدة في كل مرة
            var photo = await PickPhotoAsync();
            if (photo != null)
            {
                results.Add(photo);
            }
#endif
        }
        catch (PermissionException)
        {
            Console.WriteLine("[MauiMediaPicker] Photos permission not granted");
            await ShowErrorAlertAsync("صلاحية الصور مطلوبة",
                "يرجى السماح للتطبيق بالوصول إلى الصور من إعدادات الجهاز");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MauiMediaPicker] Error picking photos: {ex.Message}");
            await ShowErrorAlertAsync("خطأ في اختيار الصور",
                "حدث خطأ أثناء اختيار الصور. يرجى المحاولة مرة أخرى");
        }

        return results;
    }

    public async Task<List<MediaPickResult>> PickOrCapturePhotosAsync(int maxCount = 10)
    {
        var results = new List<MediaPickResult>();

        try
        {
            // عرض خيارات للمستخدم
            var action = await Application.Current!.MainPage!.DisplayActionSheet(
                "إضافة صور",
                "إلغاء",
                null,
                IsCameraAvailable ? "التقاط من الكاميرا" : null,
                "اختيار من المعرض");

            if (action == "التقاط من الكاميرا")
            {
                var photo = await CapturePhotoAsync();
                if (photo != null)
                {
                    results.Add(photo);
                }
            }
            else if (action == "اختيار من المعرض")
            {
                results = await PickPhotosAsync(maxCount);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MauiMediaPicker] Error in PickOrCapturePhotosAsync: {ex.Message}");
        }

        return results;
    }
}
