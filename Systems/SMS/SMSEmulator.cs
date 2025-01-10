using EmuHelp.HelperBase;
using System;

namespace EmuHelp.Systems.SMS;

public abstract class SMSEmulator : Emulator
{
    public IntPtr RamBase { get; protected set; }
}