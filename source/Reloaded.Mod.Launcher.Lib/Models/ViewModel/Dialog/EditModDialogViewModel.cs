﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Launcher.Lib.Commands.Mod;
using Reloaded.Mod.Launcher.Lib.Models.Model.Pages;
using Reloaded.Mod.Launcher.Lib.Models.Model.Update;
using Reloaded.Mod.Launcher.Lib.Utility;
using Reloaded.Mod.Loader.IO;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Services;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.Update;
using Reloaded.Mod.Loader.Update.Interfaces;

namespace Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog;

/// <summary>
/// The ViewModel for a dialog which allows us to edit the details of an individual mod.
/// </summary>
public class EditModDialogViewModel : Loader.IO.Utility.ObservableObject
{
    /// <summary>
    /// The individual mod configuration to be edited.
    /// </summary>
    public ModConfig Config { get; set; }

    /// <summary>
    /// The individual mod configuration to be edited.
    /// </summary>
    public PathTuple<ModConfig> ConfigTuple { get; set; }

    /// <summary>
    /// All possible dependencies for the mod configurations.
    /// </summary>
    public ObservableCollection<BooleanGenericTuple<IModConfig>> Dependencies { get; set; } = new ObservableCollection<BooleanGenericTuple<IModConfig>>();

    /// <summary>
    /// All possible applications for the mod configurations.
    /// </summary>
    public ObservableCollection<BooleanGenericTuple<IApplicationConfig>> Applications { get; set; } = new ObservableCollection<BooleanGenericTuple<IApplicationConfig>>();

    /// <summary>
    /// Filter allowing for dependencies to be filtered out.
    /// </summary>
    public string ModsFilter { get; set; } = "";

    /// <summary>
    /// The current page for the modification.
    /// </summary>
    public EditModPage Page { get; set; } = EditModPage.Main;
    
    /// <summary>
    /// True if user can navigate to the last page.
    /// </summary>
    public bool CanGoToLastPage { get; set; } = false;

    /// <summary>
    /// True if user can navigate to the next page.
    /// </summary>
    public bool CanGoToNextPage { get; set; } = true;

    /// <summary>
    /// True if user is on the last page.
    /// </summary>
    public bool IsOnLastPage { get; set; } = false;

    /// <summary>
    /// List of all configurable configurations.
    /// </summary>
    public ObservableCollection<ResolverFactoryConfiguration> Updates { get; set; } = new ObservableCollection<ResolverFactoryConfiguration>();

    private readonly ApplicationConfigService _applicationConfigService;
    private SetModImageCommand _setModImageCommand;
    private Action? _close;

    /// <inheritdoc />
    public EditModDialogViewModel(PathTuple<ModConfig> modTuple, ApplicationConfigService applicationConfigService, ModConfigService modConfigService)
    {
        _applicationConfigService = applicationConfigService;
        ConfigTuple = modTuple;
        Config = modTuple.Config;

        // Build Dependencies
        var mods = modConfigService.Items; // In case collection changes during window open.
        foreach (var mod in mods)
        {
            bool isEnabled = modTuple.Config.ModDependencies.Contains(mod.Config.ModId, StringComparer.OrdinalIgnoreCase);
            Dependencies.Add(new BooleanGenericTuple<IModConfig>(isEnabled, mod.Config));
        }

        // Build Applications
        var apps = applicationConfigService.Items;
        foreach (var app in apps)
        {
            bool isEnabled = modTuple.Config.SupportedAppId.Contains(app.Config.AppId, StringComparer.OrdinalIgnoreCase);
            Applications.Add(new BooleanGenericTuple<IApplicationConfig>(isEnabled, app.Config));
        }

        // Build Update Configurations
        foreach (var resolver in PackageResolverFactory.All)
        {
            var result = ResolverFactoryConfiguration.TryCreate(resolver, ConfigTuple);
            if (result != null)
                Updates.Add(result);
        }

        // Everything Else
        _setModImageCommand = new SetModImageCommand(modTuple);
        IoC.Kernel.Rebind<EditModDialogViewModel>().ToConstant(this);
        this.PropertyChanged += OnPageChanged;
    }

    /// <summary>
    /// Initializes the window with an action to close it.
    /// </summary>
    public void Init(Action close) => _close = close;

    /// <summary>
    /// Saves the mod back to the mod directory.
    /// </summary>
    public void Save()
    {
        // Make folder path and save folder.
        string modDirectory = Path.GetDirectoryName(ConfigTuple.Path)!;

        // Save Config
        string configSavePath  = Path.Combine(modDirectory, ModConfig.ConfigFileName);
        Config.ModDependencies = Dependencies.Where(x => x.Enabled).Select(x => x.Generic.ModId).ToArray();
        Config.SupportedAppId = Applications.Where(x => x.Enabled).Select(x => x.Generic.AppId).ToArray();

        // Save Plugins
        foreach (var update in Updates)
        {
            if (update.IsEnabled)
                update.Factory.SetConfiguration(ConfigTuple, update.Configuration);
            else
                Config.PluginData.Remove(update.Factory.ResolverId);
        }

        ConfigReader<ModConfig>.WriteConfiguration(configSavePath, (ModConfig) Config);
    }

    /* Get Image To Display */

    /// <summary>
    /// Filters an individual item.
    /// Returns true if the item should pass by the filter, else false.
    /// </summary>
    public bool FilterMod(BooleanGenericTuple<IModConfig> item)
    {
        if (ModsFilter.Length <= 0)
            return true;
        
        return item.Generic.ModName.IndexOf(this.ModsFilter, StringComparison.InvariantCultureIgnoreCase) >= 0;
    }

    /// <summary>
    /// Filters an individual item.
    /// Returns true if the item should pass by the filter, else false.
    /// </summary>
    public bool FilterApp(BooleanGenericTuple<IApplicationConfig> item)
    {
        if (ModsFilter.Length <= 0)
            return true;

        return item.Generic.AppName.IndexOf(this.ModsFilter, StringComparison.InvariantCultureIgnoreCase) >= 0;
    }

    /// <summary>
    /// Sets a new mod image.
    /// </summary>
    public void SetNewImage()
    {
        if (_setModImageCommand.CanExecute(null))
            _setModImageCommand.Execute(null);
    }

    /// <summary>
    /// Used to close the page/window associated with the dialog.
    /// </summary>
    public void Close() => _close?.Invoke();

    private void OnPageChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Page))
        {
            CanGoToLastPage = Page > EnumValues<EditModPage>.Min;
            CanGoToNextPage = Page < EnumValues<EditModPage>.Max;
            IsOnLastPage = Page == EnumValues<EditModPage>.Max;
        }
    }
}