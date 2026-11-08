using AuthGatun.ViewModels;
using Avalonia.Controls;

namespace AuthGatun.Views;

public partial class HomeView : UserControl
{
    public HomeView()
    {
        InitializeComponent();

        AttachedToVisualTree += (_, _) => (DataContext as HomeViewModel)?.StartRefreshing();
        DetachedFromVisualTree += (_, _) => (DataContext as HomeViewModel)?.StopRefreshing();
    }
}
