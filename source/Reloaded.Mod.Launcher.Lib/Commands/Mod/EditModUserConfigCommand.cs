﻿using System;
using System.Windows.Input;
using Reloaded.Mod.Launcher.Lib.Commands.Templates;
using Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog;
using Reloaded.Mod.Launcher.Lib.Static;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;

namespace Reloaded.Mod.Launcher.Lib.Commands.Mod;

/// <summary>
/// Command that allows you to spawn a dialog to edit the user configuration for a mod.
/// </summary>
public class EditModUserConfigCommand : WithCanExecuteChanged, ICommand
{
    private readonly PathTuple<ModUserConfig>? _modTuple;

    /// <summary/>
    public EditModUserConfigCommand(PathTuple<ModUserConfig>? modTuple)
    {
        _modTuple = modTuple;
    }

    /// <inheritdoc />
    public bool CanExecute(object? parameter)
    {
        return _modTuple != null;
    }

    /// <inheritdoc />
    public void Execute(object? parameter)
    {
        Actions.ShowEditModUserConfig(new EditModUserConfigDialogViewModel(_modTuple!));
    }
}
