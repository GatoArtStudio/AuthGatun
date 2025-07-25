using System;
using System.Threading;
using System.Threading.Tasks;
using AuthGatun.Domains.IdentityAccess.Domain.Aggregate;
using Avalonia.Threading;

namespace AuthGatun.Services;

public class UserStatus
{
    private UserStatus() {}

    private static UserStatus? _instance;
    private static readonly object Lock = new object();

    public User? User { get; set; }
    
    public static UserStatus GetInstance()
    {
        if (_instance is null)
        {
            lock (Lock)
            {
                if (_instance is null)
                {
                    _instance = new UserStatus();
                }
            }
        }

        return _instance;
    }

    public void RunRpcDiscord(
        string user = "AuthGatun",
        string details = "Aplicacion de autenticación TOTP, desarrollada por GatoArtStudio."
        )
    {
        string clientId = "1398030119947473037";
        var discord = new Discord.Discord(Int64.Parse(clientId), (ulong) Discord.CreateFlags.NoRequireDiscord);
        var activityManager = discord.GetActivityManager();

        var activity = new Discord.Activity
        {
            Type = Discord.ActivityType.Playing,
            State = user,
            Details = details,
            Assets =
            {
                LargeImage = "logo",
                LargeText = "AuthGatun",
                SmallImage = "logo_owner",
                SmallText = "GatoArtStudio"
            },
            Instance = false
        };
        
        activityManager.UpdateActivity(activity, result =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (result == Discord.Result.Ok)
                    NotifyManager.GetInstance().SendNotifyInWindow("Discord RPC is running successfully.", "Discord RPC");
                else
                    NotifyManager.GetInstance().SendNotifyInWindow("Discord RPC failed to start.", "Discord RPC");
            });
        });

        _ = Task.Run(() =>
        {
            while (true)
            {
                discord.RunCallbacks();
                Thread.Sleep(1000); // Update every minute
            }
        });
    }
}