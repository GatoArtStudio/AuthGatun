using System;

namespace AuthGatun.Services;

public class CodePrivacy(IClock clock, bool enabled = false)
{
    public static readonly TimeSpan RevealDuration = TimeSpan.FromSeconds(10);
    public const string Mask = "••• •••";

    private bool _hovered;
    private DateTime? _revealedUntil;

    public bool Enabled { get; private set; } = enabled;

    public bool IsRevealed => !Enabled || _hovered || IsToggleActive;

    private bool IsToggleActive => _revealedUntil is { } until && clock.UtcNow < until;

    public void SetEnabled(bool value)
    {
        Enabled = value;
        _hovered = false;
        _revealedUntil = null;
    }

    public void PointerEntered() => _hovered = true;

    public void PointerLeft()
    {
        _hovered = false;
        _revealedUntil = null;
    }

    public void ToggleReveal()
    {
        if (IsToggleActive || _hovered)
        {
            _hovered = false;
            _revealedUntil = null;
        }
        else
        {
            _revealedUntil = clock.UtcNow + RevealDuration;
        }
    }

    public string Display(string code) => IsRevealed ? code : Mask;
}
