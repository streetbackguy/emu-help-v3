using Helper.HelperBase;
using System;

namespace Helper.GBA;

public abstract class GBAEmulator : Emulator
{
    public IntPtr EWRAM { get; protected set; }
    public IntPtr IWRAM { get; protected set; }
}