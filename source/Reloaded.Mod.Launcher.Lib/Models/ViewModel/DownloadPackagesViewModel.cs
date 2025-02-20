﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using NuGet.Protocol.Core.Types;
using Reloaded.Mod.Launcher.Lib.Commands.Download;
using Reloaded.Mod.Launcher.Lib.Commands.Templates;
using Reloaded.Mod.Launcher.Lib.Models.Model.DownloadPackagePage;
using Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog;
using Reloaded.Mod.Launcher.Lib.Static;
using Reloaded.Mod.Launcher.Lib.Utility;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Services;
using Reloaded.Mod.Loader.IO.Utility;
using Reloaded.Mod.Loader.Update;
using Reloaded.Mod.Loader.Update.Interfaces;
using Reloaded.Mod.Loader.Update.Providers;
using Reloaded.Mod.Loader.Update.Providers.NuGet;
using Reloaded.Mod.Loader.Update.Utilities;
using Reloaded.Mod.Loader.Update.Utilities.Nuget;
using Reloaded.Mod.Loader.Update.Utilities.Nuget.Structs;

namespace Reloaded.Mod.Launcher.Lib.Models.ViewModel;

/// <summary>
/// ViewModel for downloading packages from multiple sources, including NuGet.
/// </summary>
public class DownloadPackagesViewModel : ObservableObject, IDisposable
{
    /// <summary>
    /// String to search mods by.
    /// </summary>
    public string SearchQuery { get; set; } = "";

    /// <summary>
    /// List of potential packages to download.
    /// </summary>
    public ObservableCollection<IDownloadablePackage> SearchResult  { get; set; } = new();

    /// <summary>
    /// The currently selected package.
    /// </summary>
    public IDownloadablePackage SelectedResult { get; set; } = null!;

    /// <summary>
    /// Status of the current package download.
    /// </summary>
    public DownloadPackageStatus DownloadPackageStatus { get; set; }

    /// <summary>
    /// Command used to download an individual mod.
    /// </summary>
    public DownloadPackageCommand DownloadModCommand { get; set; } = null!;

    /// <summary>
    /// Command used to configure sources for NuGet packages.
    /// </summary>
    public ConfigureNuGetSourcesCommand ConfigureNuGetSourcesCommand { get; set; }

    /// <summary>
    /// True if the user can go to last page, else false.
    /// </summary>
    public bool CanGoToLastPage { get; set; } = false;

    /// <summary>
    /// True if the user can go to next page, else false.
    /// </summary>
    public bool CanGoToNextPage { get; set; } = true;

    /// <summary>
    /// List of all available package providers.
    /// </summary>
    public ObservableCollection<AggregatePackageProvider> PackageProviders { get; set; } = new();

    /// <summary>
    /// The currently used package provider.
    /// </summary>
    public AggregatePackageProvider CurrentPackageProvider { get; set; }

    private CancellationTokenSource? _tokenSource;

    private PaginationHelper _paginationHelper = PaginationHelper.Default;

    /* Construction - Deconstruction */

    /// <inheritdoc />
    public DownloadPackagesViewModel(AggregateNugetRepository nugetRepository, ApplicationConfigService appConfigService)
    {
        // Get package provider for individual games.
        foreach (var appConfig in appConfigService.Items.ToArray())
        {
            var provider = PackageProviderFactory.GetProvider(appConfig);
            if (provider != null)
                PackageProviders.Add(provider);
        }

        // Get package provider for all packages.
        PackageProviders.Add(new AggregatePackageProvider(new IDownloadablePackageProvider[] { new NuGetPackageProvider(nugetRepository) }, "NuGet"));
        var allPackageProvider = new AggregatePackageProvider(PackageProviders.Select(x => (IDownloadablePackageProvider)x).ToArray(), Resources.DownloadPackagesAll.Get());
        PackageProviders.Add(allPackageProvider);
        CurrentPackageProvider = allPackageProvider;

        // Setup other viewmodel elements.
        ConfigureNuGetSourcesCommand = new ConfigureNuGetSourcesCommand(RefreshOnSourceChange);
        PropertyChanged += OnAnyPropChanged;
        UpdateCommands();

        // React to search results and pagination stuff.
        SearchResult.CollectionChanged += SetCanGoToNextPageOnSearchResultsChanged;

        // Perform Initial Search.
        _paginationHelper.ItemsPerPage = 10;
#pragma warning disable CS4014
        GetSearchResults();
#pragma warning restore CS4014
    }

    /// <summary>
    /// Gets the search results for the current search term.
    /// </summary>
    /// <returns></returns>
    public async Task GetSearchResults()
    {
        _tokenSource?.Cancel();
        _tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        
        var searchTuples = await CurrentPackageProvider.SearchAsync(SearchQuery, _paginationHelper.Skip, _paginationHelper.Take, _tokenSource.Token);
        Collections.ModifyObservableCollection(SearchResult, searchTuples);
    }

    /// <summary>
    /// Moves the search forward 1 page.
    /// </summary>
    /// <returns></returns>
    public async Task GoToNextPage()
    {
        _paginationHelper.NextPage();
        CanGoToLastPage = _paginationHelper.Page > 0;
        await GetSearchResults();
    }

    /// <summary>
    /// Moves the search back 1 page.
    /// </summary>
    public async Task GoToLastPage()
    {
        _paginationHelper.PreviousPage();
        CanGoToLastPage = _paginationHelper.Page > 0;
        await GetSearchResults();
    }

    private void OnAnyPropChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SearchQuery))
        {
            ResetSearch();
        }
        else if (e.PropertyName == nameof(CurrentPackageProvider))
        {
            ResetSearch();
        }
        else if (e.PropertyName == nameof(SelectedResult))
        {
            UpdateCommands();
        }
    }

    private void ResetSearch()
    {
        _paginationHelper.Reset();
        CanGoToLastPage = false;
#pragma warning disable 4014
        GetSearchResults(); // Fire and forget.
#pragma warning restore 4014
    }

    private void SetCanGoToNextPageOnSearchResultsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        CanGoToNextPage = SearchResult.Count >= _paginationHelper.ItemsPerPage;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _tokenSource?.Dispose();
    }

    private async void RefreshOnSourceChange() => await GetSearchResults();

    private void UpdateCommands()
    {
        DownloadModCommand = new DownloadPackageCommand(SelectedResult, this, IoC.Get<ModConfigService>());
    }
}

/// <summary>
/// Command allowing you to download an individual mod.
/// </summary>
public class DownloadPackageCommand : WithCanExecuteChanged, ICommand
{
    private readonly IDownloadablePackage? _package;
    private readonly DownloadPackagesViewModel _viewModel;
    private readonly ModConfigService _modConfigService;
    private bool _canExecute = true;
    
    /// <inheritdoc />
    public DownloadPackageCommand(IDownloadablePackage? package, DownloadPackagesViewModel viewModel, ModConfigService modConfigService)
    {
        _package = package;
        _viewModel = viewModel;
        _modConfigService = modConfigService;
    }

    /* ICommand. */

    /// <inheritdoc />
    public bool CanExecute(object? parameter)
    {
        if (!_canExecute)
            return false;

        if (_package == null)
            return ReturnResult(false, DownloadPackageStatus.Default);

        if (_modConfigService.Items.Any(x => x.Config.ModId == _package.Id))
            return ReturnResult(false, DownloadPackageStatus.AlreadyDownloaded);

        return ReturnResult(true, DownloadPackageStatus.Default);

        bool ReturnResult(bool canExecute, DownloadPackageStatus status)
        {
            _viewModel.DownloadPackageStatus = status;
            return canExecute;
        }
    }

    /// <inheritdoc />
    public void Execute(object? parameter)
    {
        _viewModel.DownloadPackageStatus = DownloadPackageStatus.Downloading;
        try
        {
            _canExecute = false;
            RaiseCanExecute(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            
            // TODO: Download Packages Async
            // Update.DownloadNuGetPackagesAsync()
            ActionWrappers.ExecuteWithApplicationDispatcher(() =>
            {
                var viewModel = new DownloadPackageViewModel(_package!, IoC.Get<LoaderConfig>());
                _ = viewModel.StartDownloadAsync(); // Fire and forget.
                Actions.ShowFetchPackageDialog(viewModel);
            });
            
            _canExecute = true;
            RaiseCanExecute(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        finally
        {
            _viewModel.DownloadPackageStatus = DownloadPackageStatus.Default;
        }
    }
}