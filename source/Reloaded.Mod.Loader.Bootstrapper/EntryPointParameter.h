﻿#pragma once

/* For C# source, see EntryPoint.cs */
#define CURRENT_VERSION 1

enum EntryPointFlags : int
{
	None = 0,
	
	/* Reloaded was initialised by an external mod loader or DLL Hijacking method. */
	LoadedExternally = 1,
};

/**
 * Defines the parameters.
 */
struct EntryPointParameters
{
public:
	int version { CURRENT_VERSION };
	EntryPointFlags flags { None };
	
	EntryPointParameters() = default;
};

DEFINE_ENUM_FLAG_OPERATORS(EntryPointFlags);