﻿using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.Update.Providers.GameBanana;
using Sewer56.DeltaPatchGenerator.Lib.Utility;
using Xunit;

namespace Reloaded.Mod.Loader.Tests.Update.Providers.GameBanana;

public class GameBananaPackageProviderTests
{
    [Fact]
    public async Task SearchAsync_WithNoString_ReturnsResults()
    {
        // Arrange
        var provider = new GameBananaPackageProvider(7486);

        // Act
        var packages = await provider.SearchAsync("");

        // Assert
        Assert.True(packages.Count > 0);
    }

    [Fact]
    public async Task SearchAsync_WithString_ReturnsMatchingResults()
    {
        // Arrange
        var provider = new GameBananaPackageProvider(7486);

        // Act
        var packages = await provider.SearchAsync("Update Lib. Test");

        // Assert
        Assert.True(packages.Count > 0);
        //Assert.Contains(packages, package => package.Id.Equals("reloaded.sharedlib.hooks", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task SearchAsync_CanDownloadReturnedPackage()
    {
        // Arrange
        var provider = new GameBananaPackageProvider(7486);
        
        // Act
        using var outputDirectory = new TemporaryFolderAllocation();
        var package = (await provider.SearchAsync("Update Lib. Test")).First();
        var downloadedPackagePath = await package.DownloadAsync(outputDirectory.FolderPath, null);

        // Assert
        Assert.True(Directory.Exists(downloadedPackagePath), "This test currently fails because GameBanana cannot distinguish between Delta and Normal packages.");
        Assert.True(File.Exists(Path.Combine(downloadedPackagePath, ModConfig.ConfigFileName)));
    }
}