using System;
using System.Linq;
using EmuHelp.Logging;
using JHelper.Common.ProcessInterop;

namespace EmuHelp.Systems.SNES.Emulators;

internal class Retroarch : SNESEmulator
{
    private IntPtr coreBaseAddress;

    private readonly string[] supportedCores =
    {
        "snes9x_libretro.dll",
        "snes9x2005_libretro.dll",
        "snes9x2010_libretro.dll",
        "mesen_libretro.dll",
    };

    internal Retroarch()
    {
        Log.Info("  => Attached to emulator: Retroarch");
    }

    public override bool FindRAM(ProcessMemory process)
    {
        // Find the loaded core module
        var core = process.Modules.FirstOrDefault(m => supportedCores.Contains(m.ModuleName));
        if (core == null)
            return false;

        coreBaseAddress = core.BaseAddress;

        // Ensure the core has the symbol retro_get_memory_data
        if (!core.Symbols.TryGetValue("retro_get_memory_data", out IntPtr baseScanAddr))
            return false;

        // Scan memory starting from retro_get_memory_data to find the RAM base
        RamBase = process.Is64Bit
            ? core.ModuleName switch
            {
                "snes9x_libretro.dll" or "snes9x2005_libretro.dll" or "snes9x2010_libretro.dll" =>
                    process.Scan(new ScanPattern(1, "05 ?? ?? ?? ?? C3")
                    {
                        OnFound = addr => process.DerefOffsets(addr + 0x4 + process.Read<int>(addr), 0x0)
                    }, baseScanAddr, 0x100),
                "mesen_libretro.dll" =>
                    process.Scan(new ScanPattern(3, "48 8D 05")
                    {
                        OnFound = addr => addr + 0x4 + process.Read<int>(addr)
                    }, baseScanAddr, 0x100),
                _ => IntPtr.Zero,
            }
            : core.ModuleName switch
            {
                "snes9x_libretro.dll" or "snes9x2005_libretro.dll" or "snes9x2010_libretro.dll" =>
                    process.Scan(new ScanPattern(1, "B8 ?? ?? ?? ?? BA") { OnFound = process.ReadPointer }, baseScanAddr, 0x100),
                "mesen_libretro.dll" =>
                    process.Scan(new ScanPattern(1, "BA ?? ?? ?? ?? B8") { OnFound = process.ReadPointer }, baseScanAddr, 0x100),
                _ => IntPtr.Zero,
            };

        if (RamBase == IntPtr.Zero)
            return false;

        Log.Info($"  => RAM address found at 0x{RamBase:X}");
        return true;
    }

    public override bool KeepAlive(ProcessMemory process)
    {
        // Simple check to see if the core module is still loaded
        return process.Read<byte>(coreBaseAddress, out _);
    }
}
