using AuthGatun.ViewModels;
using Avalonia.Controls;

namespace AuthGatun.Views;

public partial class HomeView : UserControl
{
    
    public HomeView()
    {
        InitializeComponent();
        DataContext = new HomeViewModel();
    }
}