using Helper.HelperBase;
using System;

namespace Helper.Systems.PS1;

public abstract class PS1Emulator : Emulator
{
    public IntPtr RamBase { get; protected set; }
}