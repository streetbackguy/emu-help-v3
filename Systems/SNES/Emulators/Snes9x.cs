using EmuHelp.Logging;
using JHelper.Common.ProcessInterop;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EmuHelp.Systems.SNES.Emulators;

internal class Snes9x : SNESEmulator
{
    internal Snes9x()
        : base()
    {
        Log.Info("  => Attached to emulator: Snes9x");
    }

    public override bool FindRAM(ProcessMemory process)
    {
        IntPtr ptr = IntPtr.Zero;
        ScanPattern target = new ScanPattern(16, "66 89 3C 11 80 3D ?? ?? ?? ?? 00") { OnFound = process.ReadPointer };

        ptr = process.MemoryPages.Where(p => p.RegionSize == 0x101000)
            .Select(p => process.Scan(target, p.BaseAddress, (int)p.RegionSize))
            .FirstOrDefault(p => p != IntPtr.Zero);

        if (ptr == IntPtr.Zero)
            return false;

        RamBase = ptr;

        Log.Info($"  => RAM address found at 0x{RamBase.ToString("X")}");
        return true;
    }

    public override bool KeepAlive(ProcessMemory _) => true;
}
