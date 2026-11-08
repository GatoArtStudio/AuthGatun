using System;

namespace AuthGatun.Core.Localization;

public interface ILocalizer
{
    string CurrentLanguage { get; }

    event Action? LanguageChanged;

    string Get(string key);

    string Get(string key, params object[] arguments);
}
