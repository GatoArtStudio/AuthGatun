namespace AuthGatun.Domains.Settings.Application.Ports.Outbound;

public interface ISystemLanguageProvider
{
    // Two-letter ISO code of the operating system language, or null when it cannot be determined.
    string? GetSystemLanguage();
}
