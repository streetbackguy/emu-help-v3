using EmuHelp.HelperBase;
using System;

namespace EmuHelp.Systems.SNES;

public abstract class SNESEmulator : Emulator
{
    public IntPtr RamBase { get; protected set; }
}