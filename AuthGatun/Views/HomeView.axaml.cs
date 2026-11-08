using System;
using AuthGatun.ViewModels;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;

namespace AuthGatun.Views;

public partial class HomeView : UserControl
{
    public HomeView()
    {
        InitializeComponent();

        AttachedToVisualTree += (_, _) => (DataContext as HomeViewModel)?.StartRefreshing();
        DetachedFromVisualTree += (_, _) => (DataContext as HomeViewModel)?.StopRefreshing();
    }

    private void OnCardTapped(object? sender, TappedEventArgs e)
    {
        if (e.Source is Visual source && (source.FindAncestorOfType<Button>() is not null || source.GetVisualRoot() is PopupRoot))
            return;

        ((sender as Control)?.DataContext as TotpCardViewModel)?.CopyCommand.Execute().Subscribe(_ => { }, _ => { });
    }

    private void OnCardPointerEntered(object? sender, PointerEventArgs e) =>
        ((sender as Control)?.DataContext as TotpCardViewModel)?.PointerEntered();

    private void OnCardPointerExited(object? sender, PointerEventArgs e) =>
        ((sender as Control)?.DataContext as TotpCardViewModel)?.PointerLeft();
}
