using UIKit;
using WebKit;

namespace BambuMan;

/// <summary>
/// iOS has no back button, so a swipe in from the left edge of the web view does what the Android back button does:
/// one page up (<see cref="AppNavigation.TryGoBack"/>). On the scan page it does nothing.
/// </summary>
public static class IosBackGesture
{
    public static void Attach(WKWebView webView)
    {
        var gesture = new UIScreenEdgePanGestureRecognizer { Edges = UIRectEdge.Left };

        gesture.AddTarget(() =>
        {
            if (gesture.State != UIGestureRecognizerState.Ended) return;

            AppContainer.Services?.GetService<AppNavigation>()?.TryGoBack();
        });

        webView.AddGestureRecognizer(gesture);
    }
}
