using System;
using AuthGatun.Core.Localization;
using DiscordRPC;

namespace AuthGatun.Services;

public class DiscordService(ILocalizer localizer)
{
    private DiscordRpcClient? _client;
    public string ClientId { get; private set; } = "1398030119947473037";
    public DateTime StartTime { get; private set; } = DateTime.UtcNow;
    
    public void UpdatePresence(
        string state = "AuthGatun",
        string? details = null
        )
    {
        if (_client is null)
        {
            _client = new DiscordRpcClient(ClientId);
            _client.Initialize();
        }
        
        _client.SetPresence(
            new RichPresence()
            {
                Details = details ?? localizer.Get("Presence_Default"),
                State = state,
                Timestamps = new Timestamps()
                {
                    Start = StartTime
                },
                Assets = new Assets()
                {
                    LargeImageKey = "logo",
                    LargeImageText = "AuthGatun",
                    SmallImageKey = "logo_owner",
                    SmallImageText = "GatoArtStudio"
                },
                Buttons = new Button[]
                {
                    new Button()
                    {
                        Label = localizer.Get("Presence_ProjectButton"),
                        Url = "https://github.com/GatoArtStudio/AuthGatun"
                    },
                    new Button()
                    {
                        Label = localizer.Get("Presence_DeveloperButton"),
                        Url = "https://gatoartstudio.art/"
                    }
                }
            }
        );
    }
}