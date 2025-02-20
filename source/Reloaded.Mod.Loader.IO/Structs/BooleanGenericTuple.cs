﻿using Reloaded.Mod.Loader.IO.Utility;

namespace Reloaded.Mod.Loader.IO.Structs;

public class BooleanGenericTuple<TGeneric> : ObservableObject
{
    public const string NameOfEnabled = nameof(Enabled);

    public bool Enabled { get; set; }
    public TGeneric Generic { get; set; }

    public BooleanGenericTuple(bool enabled, TGeneric generic)
    {
        Enabled = enabled;
        Generic = generic;
    }
}