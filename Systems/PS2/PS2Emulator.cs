using EmuHelp.HelperBase;
using System;

namespace EmuHelp.Systems.PS2;

public abstract class PS2Emulator : Emulator
{
    public IntPtr RamBase { get; protected set; }
}