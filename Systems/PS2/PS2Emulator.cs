using Helper.HelperBase;
using System;

namespace Helper.PS2;

public abstract class PS2Emulator : Emulator
{
    public IntPtr RamBase { get; protected set; }
}