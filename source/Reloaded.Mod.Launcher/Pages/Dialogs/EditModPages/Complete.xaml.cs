﻿using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.WPF.Theme.Default;
using Reloaded.WPF.Utilities;

namespace Reloaded.Mod.Launcher.Pages.Dialogs.EditModPages;

/// <summary>
/// Interaction logic for Special.xaml
/// </summary>
public partial class Complete : ReloadedPage
{
    public EditModDialogViewModel ViewModel { get; set; }

    private readonly CollectionViewSource _dependenciesViewSource;

    public Complete(EditModDialogViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;

        _dependenciesViewSource = new DictionaryResourceManipulator(this.Grid.Resources).Get<CollectionViewSource>("SortedApplications");
        _dependenciesViewSource.Filter += DependenciesViewSourceOnFilter;
    }

    private void DependenciesViewSourceOnFilter(object sender, FilterEventArgs e) => e.Accepted = ViewModel.FilterApp((BooleanGenericTuple<IApplicationConfig>)e.Item);

    private void AppsFilter_TextChanged(object sender, TextChangedEventArgs e) => _dependenciesViewSource.View.Refresh();
}