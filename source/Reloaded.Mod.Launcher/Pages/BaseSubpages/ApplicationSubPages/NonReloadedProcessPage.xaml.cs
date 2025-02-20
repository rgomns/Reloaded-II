﻿using Reloaded.Mod.Launcher.Lib;
using Reloaded.Mod.Launcher.Lib.Models.ViewModel.Application;
using Reloaded.Mod.Launcher.Lib.Utility;

namespace Reloaded.Mod.Launcher.Pages.BaseSubpages.ApplicationSubPages;

/// <summary>
/// Interaction logic for NonReloadedProcessPage.xaml
/// </summary>
public partial class NonReloadedProcessPage : ApplicationSubPage
{
    public NonReloadedPageViewModel ViewModel { get; set; }

    public NonReloadedProcessPage()
    {
        InitializeComponent();
        ViewModel = IoC.Get<NonReloadedPageViewModel>();
    }

    private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        var process = ViewModel.ApplicationViewModel.SelectedProcess;
        if (!process!.HasExited)
        {
            var injector = new ApplicationInjector(ViewModel.ApplicationViewModel.SelectedProcess!);
            injector.Inject();

            // Exit page.
            ViewModel.ApplicationViewModel.ChangeApplicationPage(Lib.Models.Model.Pages.ApplicationSubPage.ReloadedProcess);
        }
    }
}