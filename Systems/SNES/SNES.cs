using EmuHelp.Logging;
using JHelper.Common.ProcessInterop;
using System;
using System.Linq;

namespace EmuHelp.Systems.SNES.Emulators;

internal class SNES : SNESEmulator
{
    private const long RAM_SIZE = 0x101000; // 1 MB + 4 KB

    internal SNES()
    {
        Log.Info("  => Attached to emulator: Snes9x");
    }

    public override bool FindRAM(ProcessMemory process)
    {
        // Find a memory page of exactly RAM_SIZE
        var ramPage = process.MemoryPages.FirstOrDefault(p => p.RegionSize == RAM_SIZE);

        if (ramPage == null)
            return false;

        RamBase = ramPage.BaseAddress;
        Log.Info($"  => RAM address found at 0x{RamBase:X}");
        return true;
    }

    public override bool KeepAlive(ProcessMemory _) => true;
}