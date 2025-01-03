using Helper.HelperBase;
using System;

namespace Helper.PS1;

public abstract class PS1Emulator : Emulator
{
    public IntPtr RamBase { get; protected set; }
}