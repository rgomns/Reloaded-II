﻿using System.Diagnostics.CodeAnalysis;

namespace Reloaded.Mod.Loader.Server.Messages.Structures;

public class ModInfo
{
    public ModState State   { get; set; }
    public string ModId     { get; set; }

    public bool CanSuspend  { get; set; }
    public bool CanUnload   { get; set; }

    public ModInfo(ModState state, string modId, bool canSuspend, bool canUnload)
    {
        State = state;
        ModId = modId;
        CanSuspend = canSuspend;
        CanUnload = canUnload;
    }

    public bool CanSendSuspend
    {
        get
        {
            if (State == ModState.Running && CanSuspend)
                return true;

            return false;
        }
    }

    public bool CanSendResume
    {
        get
        {
            if (State == ModState.Suspended && CanSuspend)
                return true;

            return false;
        }
    }

    /* Autogenerated by R# */

    [ExcludeFromCodeCoverage]
    protected bool Equals(ModInfo other)
    {
        return State == other.State && 
               string.Equals(ModId, other.ModId) && 
               CanSuspend == other.CanSuspend && 
               CanUnload == other.CanUnload;
    }

    [ExcludeFromCodeCoverage]
    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (obj.GetType() != this.GetType())
            return false;

        return Equals((ModInfo)obj);
    }

    [ExcludeFromCodeCoverage]
    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = (int)State;
            hashCode = (hashCode * 397) ^ (ModId != null ? ModId.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ CanSuspend.GetHashCode();
            hashCode = (hashCode * 397) ^ CanUnload.GetHashCode();
            return hashCode;
        }
    }
}