using Helper.HelperBase;
using System;

namespace Helper.SMS;

public abstract class SMSEmulator : Emulator
{
    public IntPtr RamBase { get; protected set; }
}