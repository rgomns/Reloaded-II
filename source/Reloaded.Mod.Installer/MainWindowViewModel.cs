﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Onova.Services;
using Reloaded.Mod.Installer.DependencyInstaller;
using Reloaded.Mod.Installer.DependencyInstaller.IO;
using Reloaded.Mod.Installer.Utilities;
using static Reloaded.Mod.Installer.DependencyInstaller.DependencyInstaller;

namespace Reloaded.Mod.Installer;

public class MainWindowViewModel : ObservableObject
{
    /// <summary>
    /// The current step of the download process.
    /// </summary>
    public int CurrentStepNo { get; set; }

    /// <summary>
    /// Current production step.
    /// </summary>
    public string CurrentStepDescription { get; set; } = "";

    /// <summary>
    /// The current setup progress.
    /// </summary>
    public double Progress { get; set; }

    /// <summary>
    /// A cancellation token allowing you to cancel the download operation.
    /// </summary>
    public CancellationTokenSource CancellationToken { get; set; } = new CancellationTokenSource();

    public async Task InstallReloadedAsync(string? installFolder = null)
    {
        // Step
        installFolder ??= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "Reloaded-II");
        Directory.CreateDirectory(installFolder);

        using var tempDownloadDir = new TemporaryFolderAllocation();
        var progressSlicer = new ProgressSlicer(new Progress<double>(d =>
        { 
            Progress = d * 100.0;
        }));

        try
        {
            var downloadLocation = Path.Combine(tempDownloadDir.FolderPath, $"Reloaded-II.zip");

            // 0.15
            CurrentStepNo = 0;
            await DownloadReloadedAsync(downloadLocation, progressSlicer.Slice(0.15));
            if (CancellationToken.IsCancellationRequested)
                throw new TaskCanceledException();

            // 0.20
            CurrentStepNo = 1;
            await ExtractReloadedAsync(installFolder, downloadLocation, progressSlicer.Slice(0.05));
            if (CancellationToken.IsCancellationRequested)
                throw new TaskCanceledException();

            // 1.00
            CurrentStepNo = 2;
            await CheckAndInstallMissingRuntimesAsync(installFolder, tempDownloadDir.FolderPath,
                progressSlicer.Slice(0.8),
                s => { CurrentStepDescription = s; }, CancellationToken.Token);

            CurrentStepDescription = "Creating Shortcut";
            var executableName = IntPtr.Size == 8 ? "Reloaded-II.exe" : "Reloaded-II32.exe";
            var executablePath = Path.Combine(installFolder, executableName);
            var shortcutPath   = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "Reloaded-II.lnk");
            MakeShortcut(shortcutPath, executablePath);

            CurrentStepDescription = "All Set";
            Process.Start(executablePath);
        }
        catch (TaskCanceledException e)
        {
            IOEx.TryDeleteDirectory(installFolder);
        }
        catch (Exception e)
        {
            IOEx.TryDeleteDirectory(installFolder);
            MessageBox.Show("There was an error in installing Reloaded.\n" +
                            $"Feel free to open an issue on github.com/Reloaded-Project/Reloaded-II if you require support.\n" +
                            $"Message: {e.Message}\n" +
                            $"Stack Trace: {e.StackTrace}", "Error in Installing Reloaded");
        }
    }

    private void MakeShortcut(string shortcutPath, string executablePath)
    {
        var shell = (IShellLink)new ShellLink();
        shell.SetDescription($"Reloaded II");
        shell.SetPath($"\"{executablePath}\"");
        shell.SetWorkingDirectory(Path.GetDirectoryName(executablePath));

        // Save Shortcut
        var file = (IPersistFile)shell;
        file.Save(shortcutPath, false);
    }

    private async Task ExtractReloadedAsync(string extractFolderPath, string downloadedPackagePath, IProgress<double> slice)
    {
        CurrentStepDescription = "Extracting Reloaded II";
        var extractor = new ZipPackageExtractor();
        await extractor.ExtractPackageAsync(downloadedPackagePath, extractFolderPath, slice, CancellationToken.Token);

        if (Native.IsDirectoryEmpty(extractFolderPath))
            throw new Exception($"Reloaded failed to download (downloaded archive was not properly extracted).");

        IOEx.TryDeleteFile(downloadedPackagePath);
    }

    private async Task DownloadReloadedAsync(string downloadLocation, IProgress<double> downloadProgress)
    {
        CurrentStepDescription = "Downloading Reloaded II";
        var resolver = new GithubPackageResolver("Reloaded-Project", "Reloaded-II", "Release.zip");
        var versions = await resolver.GetPackageVersionsAsync();
        var latestVersion = versions.First();
        await resolver.DownloadPackageAsync(latestVersion, downloadLocation, downloadProgress, CancellationToken.Token);

        if (!File.Exists(downloadLocation))
            throw new Exception($"Reloaded failed to download (no file was written to disk).");
    }
}