﻿using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.Update.Interfaces;

namespace Reloaded.Mod.Loader.Update.Structures
{
    public class ResolverModPair
    {
        public IModResolver Resolver { get; set; }
        public PathTuple<ModConfig> ModTuple { get; set; }

        public ResolverModPair(IModResolver resolver, PathTuple<ModConfig> modTuple)
        {
            Resolver = resolver;
            ModTuple = modTuple;
        }
    }
}
