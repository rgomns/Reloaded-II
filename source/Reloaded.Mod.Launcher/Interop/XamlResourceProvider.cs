﻿using System.Windows;
using Reloaded.Mod.Launcher.Lib.Interop;

namespace Reloaded.Mod.Launcher.Interop;

/// <summary>
/// Resource provider which fetches resources from WPF's implementation of XAML.
/// </summary>
public class XamlResourceProvider : IDictionaryResourceProvider
{
    public TResource Get<TResource>(string resourceName) => (TResource) Application.Current.Resources[resourceName];

    public void Set<TResource>(string resourceName, TResource value) => Application.Current.Resources[resourceName] = value;

    public IDictionaryResource<TResource> GetResource<TResource>(string resourceName) => new XamlResourceEx<TResource>(resourceName);
}