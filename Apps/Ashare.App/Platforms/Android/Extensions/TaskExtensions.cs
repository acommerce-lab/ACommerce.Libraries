#if ANDROID
using Android.Gms.Tasks;

namespace Ashare.App.Platforms.Android.Extensions;

/// <summary>
/// امتدادات للتحويل من Android Task إلى .NET Task
/// </summary>
public static class TaskExtensions
{
    /// <summary>
    /// تحويل Android.Gms.Tasks.Task إلى System.Threading.Tasks.Task
    /// </summary>
    public static async Task<T> AsAsync<T>(this global::Android.Gms.Tasks.Task task) where T : Java.Lang.Object
    {
        var tcs = new TaskCompletionSource<T>();

        task.AddOnSuccessListener(new OnSuccessListener<T>(result => tcs.TrySetResult(result)));
        task.AddOnFailureListener(new OnFailureListener(ex => tcs.TrySetException(ex)));
        task.AddOnCanceledListener(new OnCanceledListener(() => tcs.TrySetCanceled()));

        return await tcs.Task;
    }

    private class OnSuccessListener<T> : Java.Lang.Object, IOnSuccessListener where T : Java.Lang.Object
    {
        private readonly Action<T> _callback;

        public OnSuccessListener(Action<T> callback)
        {
            _callback = callback;
        }

        public void OnSuccess(Java.Lang.Object result)
        {
            _callback((T)result);
        }
    }

    private class OnFailureListener : Java.Lang.Object, IOnFailureListener
    {
        private readonly Action<Exception> _callback;

        public OnFailureListener(Action<Exception> callback)
        {
            _callback = callback;
        }

        public void OnFailure(Java.Lang.Exception e)
        {
            _callback(new Exception(e.Message));
        }
    }

    private class OnCanceledListener : Java.Lang.Object, IOnCanceledListener
    {
        private readonly Action _callback;

        public OnCanceledListener(Action callback)
        {
            _callback = callback;
        }

        public void OnCanceled()
        {
            _callback();
        }
    }
}
#endif
