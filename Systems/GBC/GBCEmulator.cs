using EmuHelp.HelperBase;
using System;

namespace EmuHelp.Systems.GBC;

public abstract class GBCEmulator : Emulator
{
    public IntPtr WRAM { get; protected set; }
    public IntPtr IOHRAM { get; protected set; }
}