using System;
using DiscordRPC;

namespace AuthGatun.Services;

public class DiscordService
{
    private DiscordRpcClient? _client;
    public string ClientId { get; private set; } = "1398030119947473037";
    public DateTime StartTime { get; private set; } = DateTime.UtcNow;
    
    public void UpdatePresence(
        string state = "AuthGatun",
        string details = "Aplicacion de autenticación TOTP, desarrollada por GatoArtStudio."
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
                Details = details,
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
                        Label = "Visita nuestro proyecto",
                        Url = "https://github.com/GatoArtStudio/AuthGatun"
                    },
                    new Button()
                    {
                        Label = "Desarrollador",
                        Url = "https://gatoartstudio.art/"
                    }
                }
            }
        );
    }
}