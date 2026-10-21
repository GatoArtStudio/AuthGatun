using AuthGatun.Domains.Settings.Domain;

namespace AuthGatun.Domains.Settings.Application.Ports.Outbound;

public interface ISettingsRepository
{
    // Returns null when nothing was saved yet or the stored data is unreadable.
    AppSettings? Load();
    void Save(AppSettings settings);
}
