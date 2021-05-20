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

    static string ConvertToImageKey(string text)
    {
        return text switch
        {
            "Netflix" => "netflix",
            "Disney+" => "disneyplus",
            _ => "default"
        };
    }

    static void Main(string[] args)
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
                    UpdateActivity("Wanda Vision S1:F9", "Disney+", 300);
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
}
