using Avalonia.Controls;
using System;

namespace MoleculeLab.Views;

public partial class MainWindow : Window
{
    bool TableOpen = false;
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OpenPeriodicTable_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        TableOpen = !TableOpen;
        MainGrid.RowDefinitions = new Avalonia.Controls.RowDefinitions(TableOpen ? "300, *" : "0, *");
    }
}