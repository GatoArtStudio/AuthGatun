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
    private string _serviceName = "";
    private string _initial = "";
    private bool _isEditing;
    private string _editServiceName = "";
    private string _editSecretKey = "";
    private readonly Func<Guid, string, string?, bool>? _saveEdit;

    public string ServiceName
    {
        get => _serviceName;
        private set => this.RaiseAndSetIfChanged(ref _serviceName, value);
    }

    public string Initial
    {
        get => _initial;
        private set => this.RaiseAndSetIfChanged(ref _initial, value);
    }

    public bool IsEditing
    {
        get => _isEditing;
        set => this.RaiseAndSetIfChanged(ref _isEditing, value);
    }

    public string EditServiceName
    {
        get => _editServiceName;
        set => this.RaiseAndSetIfChanged(ref _editServiceName, value);
    }

    public string EditSecretKey
    {
        get => _editSecretKey;
        set => this.RaiseAndSetIfChanged(ref _editSecretKey, value);
    }

    public ReactiveCommand<Unit, Unit> EditCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveEditCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelEditCommand { get; }

    public ReactiveCommand<Unit, Unit> CopyCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteCommand { get; }

    public TotpCardViewModel(
        TotpCodeModel model,
        Action<Guid> copy,
        Action<Guid> delete,
        Func<Guid, string, string?, bool>? saveEdit = null)
    {
        _saveEdit = saveEdit;
        Id = model.Id;
        CopyCommand = ReactiveCommand.Create(() => copy(Id));
        DeleteCommand = ReactiveCommand.Create(() => delete(Id));
        EditCommand = ReactiveCommand.Create(() =>
        {
            EditServiceName = ServiceName;
            EditSecretKey = "";
            IsEditing = true;
        });
        SaveEditCommand = ReactiveCommand.Create(() =>
        {
            if (_saveEdit is not null && _saveEdit(Id, EditServiceName, EditSecretKey))
                IsEditing = false;
        });
        CancelEditCommand = ReactiveCommand.Create(() =>
        {
            IsEditing = false;
            EditSecretKey = "";
        });
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

    public string FormattedCode => Code.Length == 6 ? $"{Code[..3]} {Code[3..]}" : Code.Length == 8 ? $"{Code[..4]} {Code[4..]}" : Code;

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
        ServiceName = model.ServiceName;
        Initial = string.IsNullOrWhiteSpace(model.ServiceName) ? "?" : model.ServiceName.Trim()[..1].ToUpperInvariant();
        _periodSeconds = Math.Max(1, model.PeriodSeconds);
        Code = model.Code;
        SecondsRemaining = model.SecondsRemaining;
    }
}
