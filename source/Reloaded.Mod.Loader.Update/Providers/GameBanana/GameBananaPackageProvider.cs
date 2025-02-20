﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Versioning;
using Reloaded.Mod.Loader.Update.Interfaces;
using Reloaded.Mod.Loader.Update.Packaging.Extra;
using Reloaded.Mod.Loader.Update.Providers.GameBanana.Structures;
using Reloaded.Mod.Loader.Update.Providers.Web;
using Reloaded.Mod.Loader.Update.Utilities;
using Sewer56.Update.Misc;
using Sewer56.Update.Packaging.Interfaces;
using Sewer56.Update.Packaging.Structures;
using Sewer56.Update.Resolvers.GameBanana;

namespace Reloaded.Mod.Loader.Update.Providers.GameBanana;

/// <summary>
/// Provider that allows for searching of downloadable mods on GameBanana.
/// </summary>
public class GameBananaPackageProvider : IDownloadablePackageProvider
{
    private const string SourceName = "GameBanana";

    /// <summary>
    /// ID of the individual game.
    /// </summary>
    public int GameId { get; private set; }

    /// <summary/>
    public GameBananaPackageProvider(int gameId)
    {
        GameId = gameId;
    }

    /// <inheritdoc />
    public async Task<List<IDownloadablePackage>> SearchAsync(string text, int skip = 0, int take = 50, CancellationToken token = default)
    {
        // TODO: Potential bug if no manager integrated mods are returned but there are still more items to take.
        // We ignore it for now but it's best revisited in the future.

        int page       = (skip / take) + 1;
        var gbApiItems = await GameBananaMod.GetByNameAsync(text, GameId, page, take);
        var results    = new List<IDownloadablePackage>();

        if (gbApiItems == null)
            return results;

        if (!(await TryAddResultsFromReleaseMetadataAsync(gbApiItems, results)))
            AddResultsFromRawFiles(gbApiItems, results);

        return results;
    }

    private static void AddResultsFromRawFiles(List<GameBananaMod> gbApiItems, List<IDownloadablePackage> results)
    {
        foreach (var result in gbApiItems)
        {
            if (result.ManagerIntegrations == null || result.Files == null)
                continue;

            // Check manager integrations.
            int counter = 0;
            foreach (var integratedFile in result.ManagerIntegrations)
            {
                var fileId = integratedFile.Key;
                var integrations = integratedFile.Value;
                var file = result.Files.First(x => x.Id == fileId);

                // Build items.
                foreach (var integration in integrations)
                {
                    if (!integration.IsReloadedDownloadUrl().GetValueOrDefault())
                        continue;

                    var url = new Uri(integration.GetReloadedDownloadUrl());
                    var textDesc = HtmlUtilities.ConvertToPlainText(result.Description);
                    var downloadFileName = !string.IsNullOrEmpty(file.Description) ? file.Description : file.FileName;
                    var fileName = "";
                    if (counter > 0)
                    {
                        fileName = $"{result.Name!} [{counter++}]";
                    }
                    else
                    {
                        fileName = $"{result.Name!}";
                        counter++;
                    }

                    results.Add(new WebDownloadablePackage(url, false)
                    {
                        Name = fileName,
                        Description = $"[{downloadFileName}] {textDesc}",
                        Authors = GetAuthorForModItem(result),
                        FileSize = file.FileSize.GetValueOrDefault(),
                        Source = SourceName
                    });
                }
            }
        }
    }

    private static async Task<bool> TryAddResultsFromReleaseMetadataAsync(List<GameBananaMod> gbApiItems, List<IDownloadablePackage> results)
    {
        const string metadataExtension = ".json";
        const int maxFileSize = 512 * 1024; // 512KB. To prevent abuse of large JSON files.

        int resultCount = results.Count;
        using var client = new WebClient();

        foreach (var item in gbApiItems)
        {
            if (item.Files == null)
                continue;

            foreach (var file in item.Files)
            {
                if (file.FileName == null || !file.FileName.EndsWith(metadataExtension) || file.FileSize > maxFileSize || string.IsNullOrEmpty(file.DownloadUrl))
                    continue;

                // Try download metadata file.
                var metadata = await client.DownloadDataTaskAsync(new Uri(file.DownloadUrl!));
                try
                {
                    // Get metadata & filter potentially invalid file.
                    var releaseMetadata = await Singleton<ReleaseMetadata>.Instance.ReadFromDataAsync(metadata);
                    if (releaseMetadata.ExtraData == null || releaseMetadata.Releases.Count <= 0)
                        continue;

                    // Get the highest version of release.
                    var highestVersion = releaseMetadata.Releases.OrderByDescending(x => new NuGetVersion(x.Version)).First();
                    var newestRelease = releaseMetadata.GetRelease(highestVersion.Version, new ReleaseMetadataVerificationInfo());
                    if (newestRelease == null)
                        continue;

                    var url = GetDownloadUrlForFileName(newestRelease.FileName, item.Files, out var modFile);
                    if (string.IsNullOrEmpty(url))
                        continue;

                    var package = new WebDownloadablePackage(new Uri(url), false)
                    {
                        Name = item.Name!,
                        Description = HtmlUtilities.ConvertToPlainText(item.Description),
                        Authors = GetAuthorForModItem(item),
                        FileSize = modFile!.FileSize.GetValueOrDefault(),
                        Source = SourceName
                    };

                    // Get better details from extra data.
                    if (TryGetExtraData(releaseMetadata, out var extraData))
                    {
                        package.Id = !string.IsNullOrEmpty(extraData!.ModId) ? extraData.ModId : package.Name;
                        package.Name = !string.IsNullOrEmpty(extraData.ModName) ? extraData.ModName : package.Name;
                        package.Description = !string.IsNullOrEmpty(extraData.ModDescription) ? extraData.ModDescription : package.Name;
                    }

                    results.Add(package);
                }
                catch (Exception) { /* Suppress */ }
            }
        }

        return results.Count > resultCount;
    }

    private static string? GetDownloadUrlForFileName(string fileName, List<GameBananaModFile> files, out GameBananaModFile? file)
    {
        var expectedFileNames = GameBananaUtilities.GetFileNameStarts(fileName);
        file = default;
        foreach (var expectedFileName in expectedFileNames)
        {
            file = files.FirstOrDefault(x => x.FileName!.StartsWith(expectedFileName, StringComparison.OrdinalIgnoreCase));
            if (file != null)
                return file.DownloadUrl;
        }

        return null;
    }

    private static bool TryGetExtraData(ReleaseMetadata releaseMetadata, out ReleaseMetadataExtraData? extraData)
    {
        extraData = default;
        try
        {
            extraData = releaseMetadata.GetExtraData<ReleaseMetadataExtraData>();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static string GetAuthorForModItem(GameBananaMod result)
    {
        if (result.Credits == null)
            return "";

        var authors = new List<string>();
        foreach (var creditCategory in result.Credits)
        foreach (var credit in creditCategory.Value)
        {
            authors.Add(credit.Name);
        }

        string author = authors.Count switch
        {
            >= 3 => $"{authors[0]}, {authors[1]}, ...",
            <= 1 => authors[0],
            _ => $"{authors[0]}, {authors[1]}"
        };
        return author;
    }
}