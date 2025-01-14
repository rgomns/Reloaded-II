﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;
using Reloaded.Mod.Launcher.Lib;
using Reloaded.Mod.Launcher.Lib.Models.Model.Pages;
using Reloaded.Mod.Launcher.Pages;
using Reloaded.WPF.Theme.Default;

namespace Reloaded.Mod.Launcher.Converters;

[ValueConversion(typeof(PageBase), typeof(ReloadedPage))]
public class ApplicationPageBaseToPageConverter : IValueConverter
{
    public static ApplicationPageBaseToPageConverter Instance = new ApplicationPageBaseToPageConverter();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        switch ((PageBase)value)
        {
            case PageBase.None:
                return null!;

            case PageBase.Splash:
                return IoC.GetConstant<SplashPage>();

            case PageBase.Base:
                return IoC.GetConstant<BasePage>();

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