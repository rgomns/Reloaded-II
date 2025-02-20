﻿using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Versioning;
using Reloaded.Mod.Loader.Update.Interfaces;

namespace Reloaded.Mod.Loader.Tests.Update.Mocks;

[ExcludeFromCodeCoverage]
public class DummyDownloadablePackage : IDownloadablePackage
{
    public event PropertyChangedEventHandler PropertyChanged = (sender, args) => {};
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Authors { get; set; } = "";
    public string Description { get; set; } = "";
    public string Source { get; set; } = "";
    public NuGetVersion Version { get; set; } = new NuGetVersion("1.0.0");
    public long FileSize { get; } = 0;

    public Task<string> DownloadAsync(string packageFolder, IProgress<double> progress, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }
}