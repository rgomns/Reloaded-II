﻿using System.ComponentModel;
using System.Threading;
using Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog;
using Reloaded.WPF.Theme.Default;

namespace Reloaded.Mod.Launcher.Pages.Dialogs;

/// <summary>
/// Interaction logic for PublishModDialog.xaml
/// </summary>
public partial class PublishModDialog : ReloadedWindow
{
    /// <summary>
    /// ViewModel for this dialog.
    /// </summary>
    public new PublishModDialogViewModel ViewModel { get; set; }

    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    /// <inheritdoc />
    public PublishModDialog(PublishModDialogViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        this.Closing += OnClosing;
    }

    private void OnClosing(object sender, CancelEventArgs e) => _cancellationTokenSource.Cancel();

    private async void Publish_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        _cancellationTokenSource = new CancellationTokenSource();
        if (await ViewModel.BuildAsync(_cancellationTokenSource.Token))
            this.Close();
    }

    private void AddVersion_Click(object sender, System.Windows.RoutedEventArgs e) => ViewModel.AddNewVersionFolder();

    private void RemoveLastVersion_Click(object sender, System.Windows.RoutedEventArgs e) => ViewModel.RemoveSelectedVersionFolder();

    private void RemoveIgnoreRegex_Click(object sender, System.Windows.RoutedEventArgs e) => ViewModel.RemoveSelectedIgnoreRegex();

    private void AddIgnoreRegex_Click(object sender, System.Windows.RoutedEventArgs e) => ViewModel.AddIgnoreRegex();

    private void RemoveIncludeRegex_Click(object sender, System.Windows.RoutedEventArgs e) => ViewModel.RemoveSelectedIncludeRegex();

    private void AddIncludeRegex_Click(object sender, System.Windows.RoutedEventArgs e) => ViewModel.AddIncludeRegex();

    private void TestRegex_Click(object sender, System.Windows.RoutedEventArgs e) => ViewModel.ShowExcludedFiles();

    private void SetOutputFolder_Click(object sender, System.Windows.RoutedEventArgs e) => ViewModel.SetOutputFolder();

    private void PublishTutorial_Click(object sender, System.Windows.RoutedEventArgs e) => ViewModel.ShowPublishTutorial();
}