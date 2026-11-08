using AuthGatun.ViewModels;
using Avalonia.Controls;
using Avalonia.Input;

namespace AuthGatun.Views;

public partial class HomeView : UserControl
{
    public HomeView()
    {
        InitializeComponent();

        AttachedToVisualTree += (_, _) => (DataContext as HomeViewModel)?.StartRefreshing();
        DetachedFromVisualTree += (_, _) => (DataContext as HomeViewModel)?.StopRefreshing();
    }

    private void OnCardPointerEntered(object? sender, PointerEventArgs e) =>
        ((sender as Control)?.DataContext as TotpCardViewModel)?.PointerEntered();

    private void OnCardPointerExited(object? sender, PointerEventArgs e) =>
        ((sender as Control)?.DataContext as TotpCardViewModel)?.PointerLeft();
}
