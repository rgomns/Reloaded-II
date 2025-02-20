﻿using System.Windows.Input;
using NuGet.Versioning;
using Reloaded.Mod.Launcher.Lib.Commands.Templates;
using Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog;
using Reloaded.Mod.Launcher.Lib.Static;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.Update;

namespace Reloaded.Mod.Launcher.Lib.Commands.Mod;

/// <summary>
/// Command allowing you to edit the configuration file of an individual mod.
/// </summary>
public class PublishModCommand : WithCanExecuteChanged, ICommand
{
    private readonly PathTuple<ModConfig>? _modTuple;

    /// <inheritdoc />
    public PublishModCommand(PathTuple<ModConfig>? modTuple)
    {
        _modTuple = modTuple;
    }

    /* Interface Implementation */

    /// <inheritdoc />
    public bool CanExecute(object? parameter) => _modTuple != null;

    /// <inheritdoc />
    public void Execute(object? parameter)
    {
        if (!NuGetVersion.TryParse(_modTuple!.Config.ModVersion, out var version))
        {
            Actions.DisplayMessagebox(Resources.ErrorInvalidModConfigTitle.Get(), Resources.ErrorInvalidModConfigDescription.Get());
            return;
        }

        if (!PackageResolverFactory.HasAnyConfiguredResolver(_modTuple))
            Actions.DisplayMessagebox(Resources.PublishModWarningTitle.Get(), Resources.PublishModWarningDescription.Get());

        Actions.PublishModDialog(new PublishModDialogViewModel(_modTuple!));
    }
}