using Helper.Common.MemoryUtils;
using Helper.HelperBase;
using System;

namespace Helper.Wii;

public abstract class WIIEmulator : Emulator
{
    public IntPtr MEM1 { get; protected set; }
    public IntPtr MEM2 { get; protected set; }
    public Endianness Endianness { get; protected set; }
}