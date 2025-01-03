using Helper.Logging;
using JHelper.Common.ProcessInterop;
using System;

namespace Helper.SMS.Emulators;

internal class Fusion : SMSEmulator
{
    private IntPtr addr_base;

    internal Fusion()
        : base()
    {
        Log.Info("  => Attached to emulator: Fusion");
    }

    public override bool FindRAM(ProcessMemory _process)
    {
        IntPtr ptr = _process.Scan(new ScanPattern(4, "74 C8 83 3D"));
        if (ptr == IntPtr.Zero)
            return false;

        addr_base = ptr;

        if (!_process.Read(ptr, out IntPtr ptr_temp))
            return false;

        RamBase = ptr_temp;

        Log.Info($"  => RAM address found at 0x{RamBase.ToString("X")}");
        return true;
    }

    public override bool KeepAlive(ProcessMemory process)
    {
        if (!process.Read(addr_base, out IntPtr ptr))
            return false;

        RamBase = ptr;
        return true;
    }
}