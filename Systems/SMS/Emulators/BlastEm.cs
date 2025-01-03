using System;
using System.Linq;
using Helper.Common.ProcessInterop;
using Helper.Logging;

namespace Helper.SMS.Emulators;

internal class BlastEm : SMSEmulator
{
    internal BlastEm()
        : base()
    {
        Log.Info("  => Attached to emulator: BlastEm");
    }

    public override bool FindRAM(ProcessMemory _process)
    {
        IntPtr ptr = IntPtr.Zero;
        var target = new MemoryScanPattern(10, "66 81 E1 FF 1F 0F B7 C9 8A 89 ?? ?? ?? ?? C3") { OnFound = addr => _process.ReadPointer(addr) };

        ptr = _process.MemoryPages.Where(p => p.RegionSize == 0x10100)
            .Select(p => _process.Scan(target, p.BaseAddress, (int)p.RegionSize))
            .FirstOrDefault(p => p != IntPtr.Zero);

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