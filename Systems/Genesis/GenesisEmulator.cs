using Helper.Common.MemoryUtils;
using Helper.HelperBase;
using System;

namespace Helper.Genesis;

public abstract class GenesisEmulator : Emulator
{
    public IntPtr RamBase { get; protected set; }
    public Endianness Endianness { get; protected set; }
}