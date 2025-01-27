using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using AuthGatun.Views;
using Avalonia.Controls;

namespace AuthGatun.ViewModels;
public partial class MainWindowViewModel : INotifyPropertyChanged
{
    private UserControl _currentView;
    private MainWindow _mainWindow;
    
    public MainWindowViewModel(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;
        ShowHomeViewCommand = new RelayCommand(ShowHomeView);
        ShowKeysViewCommand = new RelayCommand(ShowKeysView);
        ShowSettingsViewCommand = new RelayCommand(ShowSettingsView);
        ShowHomeView();
    }
    // Seteamos la View actual
    public UserControl CurrentView
    {
        get => _currentView;
        set
        {
            _currentView = value;
            OnPropertyChanged();
        }
    }
    
    public ICommand ShowHomeViewCommand { get; }
    public ICommand ShowKeysViewCommand { get; }
    public ICommand ShowSettingsViewCommand { get; }
    
    private void ShowHomeView() => CurrentView = new HomeView(_mainWindow);
    private void ShowKeysView() => CurrentView = new KeysView();
    private void ShowSettingsView() => CurrentView = new SettingsView();
    
    public event PropertyChangedEventHandler PropertyChanged;
    
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;

        public RelayCommand(Action execute)
        {
            _execute = execute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter) => _execute();
    }
}
