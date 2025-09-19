using System;
using System.Linq;
using EmuHelp.Logging;
using EmuHelp.Systems.SNES;
using EmuHelp.Systems.SNES.Emulators;
using JHelper.Common.ProcessInterop;

namespace EmuHelp.Systems.SNES.Emulators;

internal class Snes9x : SNESEmulator
{
    internal Snes9x()
        : base()
    {
        Log.Info("  => Attached to emulator: Snes9X");
    }

    public override bool FindRAM(ProcessMemory process)
    {
        IntPtr ptr = IntPtr.Zero;
        ScanPattern target = new ScanPattern(10, "66 81 E1 FF 1F 0F B7 C9 8A 89 ?? ?? ?? ?? C3") { OnFound = process.ReadPointer };

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