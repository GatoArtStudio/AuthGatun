using System.Globalization;
using AuthGatun.Domains.Settings.Application.Ports.Outbound;

namespace AuthGatun.Domains.Settings.Adapters.Outbound.OperatingSystem;

public class CultureSystemLanguageProvider : ISystemLanguageProvider
{
    public string? GetSystemLanguage()
    {
        var language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        return string.IsNullOrWhiteSpace(language) ? null : language.ToLowerInvariant();
    }
}
