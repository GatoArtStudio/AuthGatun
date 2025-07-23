using System;
using System.Reactive;
using ReactiveUI;

namespace AuthGatun.Models;

public record UserKey(Guid Id, string ServiceName, ReactiveCommand<Guid, Unit> Command);