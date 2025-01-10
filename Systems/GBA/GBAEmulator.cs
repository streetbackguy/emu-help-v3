using EmuHelp.HelperBase;
using System;

namespace EmuHelp.Systems.GBA;

public abstract class GBAEmulator : Emulator
{
    public IntPtr EWRAM { get; protected set; }
    public IntPtr IWRAM { get; protected set; }
}