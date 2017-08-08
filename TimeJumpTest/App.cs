using System;
using System.Diagnostics;
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
        TimeSpan deltaTime;
        DateTime startMediaTime;
        DateTime startSystemTime;
        long startStopwatchTime;
        Stopwatch stopwatch;
        long minStopwatchSpan;
		long maxStopwatchSpan;


		static string filepath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Log.txt";
        static readonly StreamWriter file = new StreamWriter(filepath, true) { AutoFlush = true };

        public App()
        {
            var minSystemLabel = new Label();
            var maxSystemLabel = new Label();
            var minMediaLabel = new Label();
            var maxMediaLabel = new Label();
            var minStopwatchLabel = new Label();
			var maxStopwatchLabel = new Label();
            var deltaLabel = new Label();

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
                        minStopwatchLabel,
                        maxStopwatchLabel,
                        resetButton,
                        deltaLabel,
                    },
                },
            };

            MainPage.Appearing += async delegate {

                stopwatch = new Stopwatch();

                startSystemTime = DateTime.Now;
                startMediaTime = new DateTime((long)(CAAnimation.CurrentMediaTime() * 1e7));
                startStopwatchTime = Stopwatch.GetTimestamp();
                //pulling the start times twice, as the first initialization takes some time and distorts the measurement
            	startSystemTime = DateTime.Now;
				startMediaTime = new DateTime((long)(CAAnimation.CurrentMediaTime() * 1e7));
				startStopwatchTime = Stopwatch.GetTimestamp();
				var lastSystemTime = startSystemTime;
				var lastMediaTime = startMediaTime;
                var lastStopwatchTime = startStopwatchTime;

                Log("Initial", startSystemTime, startMediaTime, startSystemTime - lastSystemTime, startMediaTime - lastMediaTime, deltaTime);

				while (true) {

                    await Task.Run(() => Thread.Sleep(period));
                    var currentSystemTime = DateTime.Now;
                    var currentMediaTime = new DateTime((long)(CAAnimation.CurrentMediaTime() * 1e7));
					var currentStopwatchTime = Stopwatch.GetTimestamp();


					deltaTime = (currentMediaTime - startMediaTime) - (currentSystemTime - startSystemTime);


                    var elapsedSystem = currentSystemTime - lastSystemTime;
                    var elapsedMedia = currentMediaTime - lastMediaTime;
                    var elapsedStopwatch = currentStopwatchTime - lastStopwatchTime;

					//Log("Always", currentSystemTime, currentMediaTime, elapsedSystem, elapsedMedia, deltaTime);

					
                    deltaLabel.Text = string.Format("Delta: {0:0.000} ms", deltaTime.TotalMilliseconds);
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

                    if (elapsedStopwatch < minStopwatchSpan) {
                        minStopwatchSpan = elapsedStopwatch;
                        minStopwatchLabel.Text = string.Format("Min stopwatch: {0:0.0000} ms", (double)minStopwatchSpan/10000);
					}
                    if (elapsedStopwatch > maxStopwatchSpan) {
                        maxStopwatchSpan = elapsedStopwatch;
                        maxStopwatchLabel.Text = string.Format("Max stopwatch: {0:0.0000} ms", (double)maxStopwatchSpan/10000);
					}

                    if (elapsedSystem < period || elapsedMedia < period || elapsedStopwatch < period.Ticks)
                        Log("Short", currentSystemTime, currentMediaTime, elapsedSystem, elapsedMedia, deltaTime);
                    if (elapsedSystem > TimeSpan.FromMilliseconds(3 * period.TotalMilliseconds + 13) ||
                        elapsedMedia > TimeSpan.FromMilliseconds(3 * period.TotalMilliseconds + 13))
                        Log("Long ", currentSystemTime, currentMediaTime, elapsedSystem, elapsedMedia, deltaTime);

                    lastSystemTime = currentSystemTime;
                    lastMediaTime = currentMediaTime;
                    lastStopwatchTime = currentStopwatchTime;

                }

            };
        }

        public void Reset()
        {
            minSystemTimeSpan = TimeSpan.MaxValue;
            maxSystemTimeSpan = TimeSpan.MinValue;
            minMediaTimeSpan = TimeSpan.MaxValue;
            maxMediaTimeSpan = TimeSpan.MinValue;
            minStopwatchSpan = long.MaxValue;
            maxStopwatchSpan = long.MinValue;
            Log("Reset", DateTime.Now, new DateTime((long)(CAAnimation.CurrentMediaTime() * 1e7)), TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero);
			startSystemTime = DateTime.Now;
			startMediaTime = new DateTime((long)(CAAnimation.CurrentMediaTime() * 1e7));
        }

        public void Log(string type, DateTime systemTime, DateTime mediaTime, TimeSpan elapsedSystem, TimeSpan elapsedMedia, TimeSpan delta)
        {
            var message = $"{type}: ";
            message += $"SYSTEM:{elapsedSystem.TotalMilliseconds,8:F3} ms @ {systemTime.TimeOfDay}, ";
            message += $"MEDIA:{elapsedMedia.TotalMilliseconds,8:F3} ms @ {mediaTime.TimeOfDay}, ";
            message += $"DELTA: {deltaTime.TotalMilliseconds} ms";

            Console.WriteLine(message);
            file.WriteLine(message);
        }
    }
}
