using EmuHelp.HelperBase;
using JHelper.Common.MemoryUtils;
using System;

namespace EmuHelp.Systems.Xbox;

public abstract class XboxEmulator : Emulator
{
    public IntPtr RamBase { get; protected set; }
    public Endianness Endianness { get; protected set; }
}