using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CoreAnimation;
using Xamarin.Forms;

namespace TimeJumpTest
{
    public class App : Application
    {
        readonly TimeSpan period = TimeSpan.FromMilliseconds(1);

        TimeSpan minSystemTimeSpan;
        TimeSpan maxSystemTimeSpan;
        TimeSpan minMediaTimeSpan;
        TimeSpan maxMediaTimeSpan;

        static string filepath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Log.txt";
        static readonly StreamWriter file = new StreamWriter(filepath, true) { AutoFlush = true };

        public App()
        {
            var minSystemLabel = new Label();
            var maxSystemLabel = new Label();
            var minMediaLabel = new Label();
            var maxMediaLabel = new Label();

            var resetButton = new Button {
                Text = "Reset",
                Command = new Command(obj => Reset()),
            };
            Reset();

            MainPage = new ContentPage {
                Padding = 20,
                Content = new StackLayout {
                    VerticalOptions = LayoutOptions.Center,
                    Children = {
                        minSystemLabel,
                        maxSystemLabel,
                        minMediaLabel,
                        maxMediaLabel,
                        resetButton,
                    },
                },
            };

            MainPage.Appearing += async delegate {

                var lastSystemTime = DateTime.Now;
                var lastMediaTime = new DateTime((long)(CAAnimation.CurrentMediaTime() * 1e7));

                while (true) {

                    await Task.Run(() => Thread.Sleep(period));
                    var currentSystemTime = DateTime.Now;
                    var currentMediaTime = new DateTime((long)(CAAnimation.CurrentMediaTime() * 1e7));

                    var elapsedSystem = currentSystemTime - lastSystemTime;
                    var elapsedMedia = currentMediaTime - lastMediaTime;

                    if (elapsedSystem < minSystemTimeSpan) {
                        minSystemTimeSpan = elapsedSystem;
                        minSystemLabel.Text = string.Format("Min system: {0:0.000} ms", minSystemTimeSpan.TotalMilliseconds);
                    }
                    if (elapsedSystem > maxSystemTimeSpan) {
                        maxSystemTimeSpan = elapsedSystem;
                        maxSystemLabel.Text = string.Format("Max system: {0:0.000} ms", maxSystemTimeSpan.TotalMilliseconds);
                    }

                    if (elapsedMedia < minMediaTimeSpan) {
                        minMediaTimeSpan = elapsedMedia;
                        minMediaLabel.Text = string.Format("Min media: {0:0.000} ms", minMediaTimeSpan.TotalMilliseconds);
                    }
                    if (elapsedMedia > maxMediaTimeSpan) {
                        maxMediaTimeSpan = elapsedMedia;
                        maxMediaLabel.Text = string.Format("Max media: {0:0.000} ms", maxMediaTimeSpan.TotalMilliseconds);
                    }

                    if (elapsedSystem < period || elapsedMedia < period)
                        Log("Short", currentSystemTime, currentMediaTime, elapsedSystem, elapsedMedia);
                    if (elapsedSystem > TimeSpan.FromMilliseconds(10 * period.TotalMilliseconds) ||
                        elapsedMedia > TimeSpan.FromMilliseconds(10 * period.TotalMilliseconds))
                        Log("Long ", currentSystemTime, currentMediaTime, elapsedSystem, elapsedMedia);

                    lastSystemTime = currentSystemTime;
                    lastMediaTime = currentMediaTime;

                }

            };
        }

        public void Reset()
        {
            minSystemTimeSpan = TimeSpan.MaxValue;
            maxSystemTimeSpan = TimeSpan.MinValue;
            minMediaTimeSpan = TimeSpan.MaxValue;
            maxMediaTimeSpan = TimeSpan.MinValue;
        }

        public void Log(string type, DateTime systemTime, DateTime mediaTime, TimeSpan elapsedSystem, TimeSpan elapsedMedia)
        {
            var message = $"{type}: ";
            message += $"SYSTEM: {systemTime.TimeOfDay}, {elapsedSystem.TotalMilliseconds:0.000} ms ";
            message += $"MEDIA: {mediaTime.TimeOfDay}, {elapsedMedia.TotalMilliseconds:0.000} ms";

            Console.WriteLine(message);
            file.WriteLine(message);
        }
    }
}
