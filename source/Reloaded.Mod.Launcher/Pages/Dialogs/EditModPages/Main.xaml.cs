﻿using System.Windows.Input;
using Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog;
using Reloaded.WPF.Theme.Default;

namespace Reloaded.Mod.Launcher.Pages.Dialogs.EditModPages;

/// <summary>
/// Interaction logic for Main.xaml
/// </summary>
public partial class Main : ReloadedPage
{
    public EditModDialogViewModel ViewModel { get; set; }

    public Main(EditModDialogViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
    }

    private void ModIcon_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed)
            return;

        ViewModel.SetNewImage();
    }
}