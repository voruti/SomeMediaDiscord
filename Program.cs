using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Control;

class Program
{
    static Discord.ActivityManager activityManager;

    static void UpdateActivity(string textA, string textB, long progress)
    {
        var activity = new Discord.Activity
        {
            State = textA,
            Details = textB,
            Timestamps =
            {
                Start = DateTimeOffset.Now.ToUnixTimeSeconds() - progress
            },
            Assets =
            {
                LargeImage = ConvertToImageKey(textB),
                LargeText = textB,
                SmallImage = ConvertToImageKey(textA),
                SmallText = textA
            },
            Instance = false
        };

        activityManager.UpdateActivity(activity, result =>
        {
            if (result != Discord.Result.Ok)
            {
                Console.WriteLine("Update Activity {0}", result);
            }
        });
    }
    static void ClearActivity()
    {

        activityManager.ClearActivity(result =>
        {
            if (result != Discord.Result.Ok)
            {
                Console.WriteLine("Clear Activity {0}", result);
            }
        });
    }

    static string ConvertToImageKey(string text)
    {
        if (text.EndsWith("Disney+"))
        {
            return "disneyplus";
        }
        return text switch
        {
            "Netflix" => "netflix",
            "Disney+" => "disneyplus",
            "Firefox" => "firefox",
            "Microsoft Edge" => "edge",
            "VLC" => "vlc",
            _ => "default"
        };
    }

    static async Task Main(string[] args)
    {
        var applicationClientID = "844635364667818024";
        var discord = new Discord.Discord(Int64.Parse(applicationClientID), (UInt64)Discord.CreateFlags.Default);

        activityManager = discord.GetActivityManager();

        try
        {
            for (long i = 0; true; i++)
            {
                if (i % 900 == 0)
                {
                    var session = await GetSession();
                    if (session != null && session.GetPlaybackInfo().PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing)
                    {
                        var timelineProperties = session.GetTimelineProperties();
                        var lastUpdate = timelineProperties.LastUpdatedTime;
                        var totalSeconds = Convert.ToInt64(timelineProperties.Position.TotalSeconds);
                        var updateAge = DateTimeOffset.Now.ToUnixTimeSeconds() - lastUpdate.ToUnixTimeSeconds();
                        var resultingPosition = totalSeconds + updateAge;
                        if (lastUpdate.ToUnixTimeSeconds() <= 0)
                        {
                            resultingPosition = 0;
                        }
                        // Console.WriteLine(totalSeconds + " at: " + lastUpdate + " with delay of: " + updateAge + " resulting position: " + resultingPosition);
                        UpdateActivity((await session.TryGetMediaPropertiesAsync()).Title, "Media", resultingPosition);
                    }
                    else
                    {
                        ClearActivity();
                    }
                }

                discord.RunCallbacks();
                Thread.Sleep(1000 / 60);
            }
        }
        finally
        {
            discord.Dispose();
        }
    }

    static async Task<GlobalSystemMediaTransportControlsSession> GetSession()
    {
        return (await GlobalSystemMediaTransportControlsSessionManager.RequestAsync()).GetCurrentSession();
    }
}
