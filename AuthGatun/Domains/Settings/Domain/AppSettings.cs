using System;
using System.Collections.Generic;
using System.Linq;

namespace AuthGatun.Domains.Settings.Domain;

public sealed record AppSettings
{
    public static readonly IReadOnlyList<int> AllowedAutoLockMinutes = [0, 1, 5, 15, 30];

    public static AppSettings Default => new(false, 5);

    public bool RunInBackground { get; }
    public int AutoLockMinutes { get; }

    public AppSettings(bool runInBackground, int autoLockMinutes)
    {
        if (!AllowedAutoLockMinutes.Contains(autoLockMinutes))
            throw new ArgumentException("The auto lock time is not one of the allowed values.", nameof(autoLockMinutes));

        RunInBackground = runInBackground;
        AutoLockMinutes = autoLockMinutes;
    }
}
