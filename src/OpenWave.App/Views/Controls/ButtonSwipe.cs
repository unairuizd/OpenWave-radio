namespace OpenWave.App.Views.Controls;

/// <summary>
/// On Android a Button takes the touch, so a swipe recognizer on its parent never sees a swipe that starts on the
/// button. This reads the swipe on the button itself: once the finger has moved far enough along the axis, the
/// button's press is cancelled (no click) and lifting the finger reports the swipe. Other platforms need nothing.
/// </summary>
internal static class ButtonSwipe
{
    /// <param name="onSwipe">Called with +1 for a swipe up or left, and -1 for a swipe down or right.</param>
    public static void Attach(Button button, bool horizontal, Action<int> onSwipe)
    {
#if ANDROID
        button.HandlerChanged += (_, _) => AttachPlatform(button, horizontal, onSwipe);
#endif
    }

#if ANDROID
    private static void AttachPlatform(Button button, bool horizontal, Action<int> onSwipe)
    {
        if (button.Handler?.PlatformView is not Android.Views.View view)
        {
            return;
        }

        var threshold = Android.Views.ViewConfiguration.Get(view.Context!)!.ScaledTouchSlop * 3;
        var start = 0f;
        var swiping = false;
        view.Touch += (_, e) =>
        {
            var motion = e.Event!;
            var position = horizontal ? motion.RawX : motion.RawY;
            e.Handled = false;
            switch (motion.ActionMasked)
            {
                case Android.Views.MotionEventActions.Down:
                    start = position;
                    swiping = false;
                    break;
                case Android.Views.MotionEventActions.Move:
                    if (!swiping && Math.Abs(position - start) > threshold)
                    {
                        swiping = true;
                        var cancel = Android.Views.MotionEvent.Obtain(motion)!;
                        cancel.Action = Android.Views.MotionEventActions.Cancel;
                        view.OnTouchEvent(cancel);
                        cancel.Recycle();
                    }

                    e.Handled = swiping;
                    break;
                case Android.Views.MotionEventActions.Up when swiping:
                    e.Handled = true;
                    swiping = false;
                    onSwipe(position < start ? 1 : -1);
                    break;
                case Android.Views.MotionEventActions.Cancel:
                    swiping = false;
                    break;
            }
        };
    }
#endif
}
