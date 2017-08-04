using System;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace TimeJumpTest
{
    public class App : Application
    {
        readonly TimeSpan period = TimeSpan.FromMilliseconds(1);

        public App()
        {
            var minLabel = new Label();
            var maxLabel = new Label();

            var minTimeSpan = TimeSpan.MaxValue;
            var maxTimeSpan = TimeSpan.MinValue;

            MainPage = new ContentPage {
                Padding = 20,
                Content = new StackLayout {
                    VerticalOptions = LayoutOptions.Center,
                    Children = {
                        minLabel,
                        maxLabel,
                        new Button {
                            Text = "Reset",
                            Command = new Command(obj => {
                                minTimeSpan = TimeSpan.MaxValue;
                                maxTimeSpan = TimeSpan.MinValue;
                            }),
                        },
                    },
                },
            };

            MainPage.Appearing += async delegate {

                var lastTime = DateTime.Now;

                while (true) {

                    await Task.Run(() => Thread.Sleep(period));
                    var currentTime = DateTime.Now;

                    var elapsed = currentTime - lastTime;
                    if (elapsed < minTimeSpan) {
                        minTimeSpan = elapsed;
                        minLabel.Text = string.Format("Min: {0:0.000} ms", minTimeSpan.TotalMilliseconds);
                    }
                    if (elapsed > maxTimeSpan) {
                        maxTimeSpan = elapsed;
                        maxLabel.Text = string.Format("Max: {0:0.000} ms", maxTimeSpan.TotalMilliseconds);
                    }

                    if (elapsed < period)
                        Console.WriteLine("Short timespan: {0:0.000} ms elapsed", elapsed.TotalMilliseconds);
                    if (elapsed > TimeSpan.FromMilliseconds(10 * period.TotalMilliseconds))
                        Console.WriteLine("Long timespan: {0:0.000} ms elapsed", elapsed.TotalMilliseconds);

                    lastTime = currentTime;
                }

            };
        }
    }
}
