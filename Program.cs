using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Control;

class Program
{
    static Discord.ActivityManager activityManager;

    static void UpdateActivity(string content, string program, long progress)
    {
        var activity = new Discord.Activity
        {
            State = content,
            Details = program,
            Timestamps =
            {
                Start = DateTimeOffset.Now.ToUnixTimeSeconds() - progress
            },
            Assets =
            {
                LargeImage = ConvertToImageKey(program),
                LargeText = program,
                SmallImage = ConvertToImageKey(content),
                SmallText = content
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
        return text switch
        {
            "Netflix" => "netflix",
            "Disney+" => "disneyplus",
            _ => "default"
        };
    }

    static Task Main(string[] args)
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
                    GetMediaProperties().ContinueWith(task =>
                    {
                        if (task.Result != null)
                        {
                            UpdateActivity(task.Result.Title, "Disney+", 30);
                        }
                        else
                        {
                            ClearActivity();
                        }
                    });
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

    static async Task<GlobalSystemMediaTransportControlsSessionMediaProperties> GetMediaProperties()
    {
        // from https://stackoverflow.com/a/63099881:
        var gsmtcsm = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
        var session = gsmtcsm.GetCurrentSession();
        if (session != null)
        {
            return await session.TryGetMediaPropertiesAsync();
        }
        return null;
    }
}
