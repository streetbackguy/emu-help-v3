using System;
using Helper.Logging;
using JHelper.Common.ProcessInterop;

namespace Helper.SMS.Emulators;

internal class Mednafen : SMSEmulator
{
    internal Mednafen()
        : base()
    {
        Log.Info("  => Attached to emulator: Mednafen");
    }

    public override bool FindRAM(ProcessMemory _process)
    {
        IntPtr ptr = _process.Scan(_process.Is64Bit
            ? new MemoryScanPattern(7, "25 FF 1F 00 00 88 90") { OnFound = addr => (IntPtr)_process.Read<int>(addr) }
            : new MemoryScanPattern(8, "25 FF 1F 00 00 0F B6 80") { OnFound = addr => (IntPtr)_process.Read<int>(addr) });

        if (ptr == IntPtr.Zero)
            return false;

        RamBase = ptr;

        Log.Info($"  => RAM address found at 0x{RamBase.ToString("X")}");
        return true;
    }

    public override bool KeepAlive(ProcessMemory _)
    {
        return true;
    }
}