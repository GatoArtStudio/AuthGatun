using System;
using System.Collections.Generic;
using System.Globalization;
using AuthGatun.Core.Localization;
using Avalonia;

namespace AuthGatun.Services.Localization;

public class Localizer : ILocalizer
{
    private readonly AppSettingsState _settings;
    private IReadOnlyDictionary<string, string> _table = StringsEnglish.Table;

    public string CurrentLanguage { get; private set; } = "en";

    public event Action? LanguageChanged;

    public Localizer(AppSettingsState settings)
    {
        _settings = settings;
        Load(settings.Current.Language);
        settings.Changed += OnSettingsChanged;
    }

    public string Get(string key) =>
        _table.TryGetValue(key, out var value) ? value : StringsEnglish.Table.GetValueOrDefault(key, key);

    public string Get(string key, params object[] arguments) =>
        string.Format(CultureInfo.CurrentCulture, Get(key), arguments);

    public static IReadOnlyDictionary<string, string> TableFor(string language) =>
        language == "es" ? StringsSpanish.Table : StringsEnglish.Table;

    private void OnSettingsChanged()
    {
        if (_settings.Current.Language == CurrentLanguage)
            return;

        Load(_settings.Current.Language);
        LanguageChanged?.Invoke();
    }

    private void Load(string language)
    {
        CurrentLanguage = language;
        _table = TableFor(language);

        var resources = Application.Current?.Resources;
        if (resources is null)
            return;

        foreach (var (key, value) in _table)
            resources[key] = value;
    }
}
