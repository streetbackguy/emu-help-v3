using System;
using Helper.Logging;
using JHelper.Common.ProcessInterop;

namespace Helper.PS1.Emulators;

internal class ePSXe : PS1Emulator
{
    internal ePSXe()
        : base()
    {
        Log.Info("  => Attached to emulator: ePSXe");
    }

    public override bool FindRAM(ProcessMemory process)
    {
        IntPtr ptr = process.Scan(new MemoryScanPattern(5, "C1 E1 10 8D 89") { OnFound = addr => process.ReadPointer(addr) });
        
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