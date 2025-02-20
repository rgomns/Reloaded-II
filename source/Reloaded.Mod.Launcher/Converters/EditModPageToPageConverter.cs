﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;
using Reloaded.Mod.Launcher.Lib;
using Reloaded.Mod.Launcher.Lib.Models.Model.Pages;
using Reloaded.Mod.Launcher.Pages.Dialogs.EditModPages;
using Reloaded.WPF.Theme.Default;

namespace Reloaded.Mod.Launcher.Converters;

[ValueConversion(typeof(EditModPage), typeof(ReloadedPage))]
public class EditModPageToPageConverter : IValueConverter
{
    public static EditModPageToPageConverter Instance = new EditModPageToPageConverter();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        switch ((EditModPage)value)
        {
            case EditModPage.Main:
                return IoC.Get<Main>();
            case EditModPage.Dependencies:
                return IoC.Get<Dependencies>();
            case EditModPage.Updates:
                return IoC.Get<Updates>();
            case EditModPage.Special:
                return IoC.Get<Complete>();
            default:
                Debugger.Break();
                return null!;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}