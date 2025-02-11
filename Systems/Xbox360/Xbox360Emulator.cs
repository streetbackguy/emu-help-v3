using EmuHelp.HelperBase;
using JHelper.Common.MemoryUtils;
using System;

namespace EmuHelp.Systems.Xbox360;

public abstract class Xbox360Emulator : Emulator
{
    public IntPtr RamBase { get; protected set; }
    public Endianness Endianness { get; protected set; }
}