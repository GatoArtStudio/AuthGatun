using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using AuthGatun.Domains.Settings.Application.Ports.Outbound;
using AuthGatun.Domains.Settings.Domain;

namespace AuthGatun.Domains.Settings.Adapters.Outbound.JsonFile;

public partial class JsonFileSettingsRepository(string filePath) : ISettingsRepository
{
    public AppSettings? Load()
    {
        if (!File.Exists(filePath))
            return null;

        try
        {
            var stored = JsonSerializer.Deserialize(File.ReadAllBytes(filePath), SettingsJsonContext.Default.StoredSettings);
            return stored is null ? null : new AppSettings(stored.RunInBackground, stored.AutoLockMinutes);
        }
        catch (Exception e) when (e is JsonException or ArgumentException or IOException)
        {
            return null;
        }
    }

    public void Save(AppSettings settings)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        var temporaryPath = filePath + ".tmp";
        var stored = new StoredSettings(settings.RunInBackground, settings.AutoLockMinutes);
        File.WriteAllBytes(temporaryPath, JsonSerializer.SerializeToUtf8Bytes(stored, SettingsJsonContext.Default.StoredSettings));
        File.Move(temporaryPath, filePath, true);
    }

    internal sealed record StoredSettings(bool RunInBackground, int AutoLockMinutes);

    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(StoredSettings))]
    internal sealed partial class SettingsJsonContext : JsonSerializerContext;
}
