using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using User = AuthGatun.Domains.IdentityAccess.Domain.Aggregate.User;

namespace AuthGatun.Services;

public class UserStatus
{
    private UserStatus() {}

    private static UserStatus? _instance;
    private static readonly object Lock = new object();

    public User? User { get; set; }
    
    public bool IsRunningRpcDiscord { get; private set; } = false;
    private Discord.Discord? _discord;
    private Task? _rpcLoopTask;
    private CancellationTokenSource? _ctsRpc;
    private long _startTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    
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
        
        if (!IsRunningRpcDiscord)
        {
            _discord = new Discord.Discord(Int64.Parse(clientId), (ulong) CreateFlags.NoRequireDiscord);
            _ctsRpc = new CancellationTokenSource();
            var token = _ctsRpc.Token;
            
            _rpcLoopTask = Task.Run(() =>
            {
                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        _discord.RunCallbacks();
                        Thread.Sleep(1000); // Update every second
                    }
                }
                catch (Exception e)
                {
                    // ignore
                }
            }, token);

            IsRunningRpcDiscord = true;
        }
        
        var activityManager = _discord?.GetActivityManager();
        if (activityManager is null) return; 

        var activity = new Activity
        {
            Type = ActivityType.Playing,
            State = user,
            Details = details,
            Timestamps =
            {
                Start = _startTimestamp
            },
            Assets =
            {
                LargeImage = "logo",
                LargeText = "AuthGatun",
                SmallImage = "logo_owner",
                SmallText = "GatoArtStudio"
            },
            Instance = false
        };
        
        activityManager.UpdateActivity(activity, result => {});
    }
}