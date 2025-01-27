using Avalonia.Controls;
using AuthGatun.ViewModels;

namespace AuthGatun.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel(this);
    }
}