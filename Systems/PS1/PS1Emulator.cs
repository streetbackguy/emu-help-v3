using EmuHelp.HelperBase;
using System;

namespace EmuHelp.Systems.PS1;

public abstract class PS1Emulator : Emulator
{
    public IntPtr RamBase { get; protected set; }
}