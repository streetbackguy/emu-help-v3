using EmuHelp.HelperBase;
using JHelper.Common.MemoryUtils;
using System;

namespace EmuHelp.Systems.Wii;

public abstract class WIIEmulator : Emulator
{
    public IntPtr MEM1 { get; protected set; }
    public IntPtr MEM2 { get; protected set; }
    public Endianness Endianness { get; protected set; }
}