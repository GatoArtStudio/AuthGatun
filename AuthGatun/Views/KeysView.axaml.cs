using System.Linq;
using AuthGatun.ViewModels;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;

namespace AuthGatun.Views;

public partial class KeysView : UserControl
{
    public KeysView()
    {
        InitializeComponent();

        AddHandler(DragDrop.DragOverEvent, OnDragOver);
        AddHandler(DragDrop.DropEvent, OnDrop);
    }

    private static void OnDragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = e.Data.Contains(DataFormats.Files) ? DragDropEffects.Copy : DragDropEffects.None;
    }

    private async void OnDrop(object? sender, DragEventArgs e)
    {
        if (DataContext is not KeyViewModel viewModel)
            return;

        var file = e.Data.GetFiles()?.OfType<IStorageFile>().FirstOrDefault();
        if (file is not null)
            await viewModel.ScanDroppedFileAsync(file);
    }
}
