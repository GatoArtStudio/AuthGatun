using AuthGatun.Core;
using Avalonia.Controls;
using AuthGatun.ViewModels;

namespace AuthGatun.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel(this);
        
        // NotifyManager.GetInstance().MainWindow = this;
        NotifyManager notifyManager = NotifyManager.GetInstance();
        notifyManager.MainWindow = this;
    }
}