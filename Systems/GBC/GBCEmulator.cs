using Helper.HelperBase;
using System;

namespace Helper.GBC;

public abstract class GBCEmulator : Emulator
{
    public IntPtr WRAM { get; protected set; }
    public IntPtr IOHRAM { get; protected set; }
}