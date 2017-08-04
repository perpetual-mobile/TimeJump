using System;
using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

namespace TimeJumpTest.iOS
{
    [Register("AppDelegate")]
    public class AppDelegate : FormsApplicationDelegate
    {
        nint taskId;

        public override bool FinishedLaunching(UIApplication uiApplication, NSDictionary launchOptions)
        {
            UIApplication.SharedApplication.IdleTimerDisabled = true;

            Forms.Init();

            LoadApplication(new App());

            return base.FinishedLaunching(uiApplication, launchOptions);
        }

        public override void DidEnterBackground(UIApplication uiApplication)
        {
            base.DidEnterBackground(uiApplication);

            taskId = UIApplication.SharedApplication.BeginBackgroundTask(() => {
                UIApplication.SharedApplication.EndBackgroundTask(taskId);
            });
        }

        public override void WillEnterForeground(UIApplication uiApplication)
        {
            UIApplication.SharedApplication.EndBackgroundTask(taskId);

            base.WillEnterForeground(uiApplication);
        }
    }
}
