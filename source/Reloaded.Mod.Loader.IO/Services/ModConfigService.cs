﻿using System;
using System.Collections.Generic;
using System.Threading;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;

namespace Reloaded.Mod.Loader.IO.Services
{
    /// <summary>
    /// Service which provides access to various mod configurations.
    /// </summary>
    public class ModConfigService : ConfigServiceBase<ModConfig>
    {
        /// <summary>
        /// All mod user configs by their unique ID.
        /// </summary>
        public Dictionary<string, PathTuple<ModConfig>> ItemsById { get; private set; } = new Dictionary<string, PathTuple<ModConfig>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Creates the service instance given an instance of the configuration.
        /// </summary>
        /// <param name="config">Mod loader config.</param>
        /// <param name="context">Context to which background events should be synchronized.</param>
        public ModConfigService(LoaderConfig config, SynchronizationContext context = null)
        {
            this.OnAddItem += OnAddItemHandler;
            this.OnRemoveItem += OnRemoveItemHandler;

            Initialize(config.ModConfigDirectory, ModConfig.ConfigFileName, GetAllConfigs, context);
            SetItemsById();
        }
        private void SetItemsById()
        {
            foreach (var item in Items)
                ItemsById[item.Config.ModId] = item;
        }

        private void OnRemoveItemHandler(PathTuple<ModConfig> obj) => ItemsById.Remove(obj.Config.ModId);

        private void OnAddItemHandler(PathTuple<ModConfig> obj) => ItemsById[obj.Config.ModId] = obj;

        private List<PathTuple<ModConfig>> GetAllConfigs() => ModConfig.GetAllMods(base.ConfigDirectory);
    }
}
