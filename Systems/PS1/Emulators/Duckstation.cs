using Helper.Logging;
using JHelper.Common.ProcessInterop;
using System;

namespace Helper.Systems.PS1.Emulators;

internal class Duckstation : PS1Emulator
{
    private IntPtr ramPointer = IntPtr.Zero;

    internal Duckstation()
        : base()
    {
        Log.Info("  => Attached to emulator: Duckstation");
    }

    public override bool FindRAM(ProcessMemory _process)
    {
        if (!_process.Is64Bit)
            return false;

        // If the ramPointer value has been already found, there
        // is no need for the more complex memory stuff below
        if (ramPointer != IntPtr.Zero)
            return true;

        // Evaluate and calculate the value of the main pointer to WRAM
        if (_process.MainModule.Symbols.TryGetValue("RAM", out IntPtr symbol))
        {
            ramPointer = symbol;
        }
        // Old Duckstation releases don't use debug symbols. In this case, we employ a dedicated signature scan
        else
        {
            ramPointer = _process.MainModule.Scan(new MemoryScanPattern(3, "48 89 0D ?? ?? ?? ?? B8") { OnFound = addr => addr + 0x4 + _process.Read<int>(addr) });
            if (ramPointer == IntPtr.Zero)
                return false;
        }

        if (!_process.Read(ramPointer, out IntPtr ptr))
            return false;

        RamBase = ptr;

        if (RamBase != IntPtr.Zero)
            Log.Info($"  => RAM address found at 0x{RamBase.ToString("X")}");

        return true;
    }

    public override bool KeepAlive(ProcessMemory process)
    {
        if (!process.Read(ramPointer, out IntPtr ptr))
            return false;

        RamBase = ptr;
        return true;
    }
}