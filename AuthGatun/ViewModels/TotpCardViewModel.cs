using System;
using System.Reactive;
using AuthGatun.Domains.IdentityAccess.Application.Models;
using ReactiveUI;

namespace AuthGatun.ViewModels;

public class TotpCardViewModel : ViewModelBase
{
    private const int WarningSeconds = 10;
    private const int CriticalSeconds = 5;

    private string _code = "";
    private int _secondsRemaining;
    private int _periodSeconds = 30;

    public Guid Id { get; }
    public string ServiceName { get; }
    public string Initial { get; }

    public ReactiveCommand<Unit, Unit> CopyCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteCommand { get; }

    public TotpCardViewModel(
        TotpCodeModel model,
        Action<Guid> copy,
        Action<Guid> delete)
    {
        Id = model.Id;
        ServiceName = model.ServiceName;
        Initial = string.IsNullOrWhiteSpace(model.ServiceName) ? "?" : model.ServiceName.Trim()[..1].ToUpperInvariant();
        CopyCommand = ReactiveCommand.Create(() => copy(Id));
        DeleteCommand = ReactiveCommand.Create(() => delete(Id));
        Update(model);
    }

    public string Code
    {
        get => _code;
        private set
        {
            this.RaiseAndSetIfChanged(ref _code, value);
            this.RaisePropertyChanged(nameof(FormattedCode));
        }
    }

    public string FormattedCode => Code.Length == 6 ? $"{Code[..3]} {Code[3..]}" : Code;

    public int SecondsRemaining
    {
        get => _secondsRemaining;
        private set
        {
            this.RaiseAndSetIfChanged(ref _secondsRemaining, value);
            this.RaisePropertyChanged(nameof(SweepAngle));
            this.RaisePropertyChanged(nameof(IsExpiring));
            this.RaisePropertyChanged(nameof(IsCritical));
        }
    }

    public double SweepAngle => Math.Clamp(360.0 * _secondsRemaining / _periodSeconds, 1, 359.9);

    public bool IsExpiring => _secondsRemaining <= WarningSeconds && _secondsRemaining > CriticalSeconds;

    public bool IsCritical => _secondsRemaining <= CriticalSeconds;

    public void Update(TotpCodeModel model)
    {
        _periodSeconds = Math.Max(1, model.PeriodSeconds);
        Code = model.Code;
        SecondsRemaining = model.SecondsRemaining;
    }
}
