﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;
using Reloaded.Mod.Launcher.Lib;
using Reloaded.Mod.Launcher.Pages.BaseSubpages.ApplicationSubPages;
using Reloaded.WPF.Theme.Default;
using ApplicationSubPage = Reloaded.Mod.Launcher.Lib.Models.Model.Pages.ApplicationSubPage;

namespace Reloaded.Mod.Launcher.Converters;

[ValueConversion(typeof(ApplicationSubPage), typeof(ReloadedPage))]
public class ApplicationSubPageToPageConverter : IValueConverter
{
    public static ApplicationSubPageToPageConverter Instance = new ApplicationSubPageToPageConverter();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        switch ((ApplicationSubPage)value)
        {
            case ApplicationSubPage.Null:
                return null!;
            case ApplicationSubPage.NonReloadedProcess:
                return IoC.Get<NonReloadedProcessPage>();
            case ApplicationSubPage.ReloadedProcess:
                return IoC.Get<ReloadedProcessPage>();
            case ApplicationSubPage.ApplicationSummary:
                return IoC.Get<AppSummaryPage>();
            case ApplicationSubPage.EditApplication:
                return IoC.Get<EditAppPage>();
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