using JHelper.Common.MemoryUtils;
using EmuHelp.HelperBase;
using System;

namespace EmuHelp.Systems.Genesis;

public abstract class GenesisEmulator : Emulator
{
    public IntPtr RamBase { get; protected set; }
    public Endianness Endianness { get; protected set; }
}