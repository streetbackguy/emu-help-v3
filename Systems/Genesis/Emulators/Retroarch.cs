using System;
using System.Linq;
using Helper.Logging;
using JHelper.Common.MemoryUtils;
using JHelper.Common.ProcessInterop;

namespace Helper.Genesis.Emulators;

internal class Retroarch : GenesisEmulator
{
    private IntPtr core_base_address;
    private readonly string[] supportedCores =
    [
        "blastem_libretro.dll",
        "genesis_plus_gx_libretro.dll",
        "genesis_plus_gx_wide_libretro.dll",
        "picodrive_libretro.dll",
    ];

    internal Retroarch()
        : base()
    {
        Endianness = Endianness.Little;
        Log.Info("  => Attached to emulator: Retroarch");
    }

    public override bool FindRAM(ProcessMemory _process)
    {
        var currentCore = _process.Modules.FirstOrDefault(m => supportedCores.Contains(m.ModuleName));
        if (currentCore == null)
            return false;

        core_base_address = currentCore.BaseAddress;

        if (currentCore.ModuleName == "blastem_libretro.dll")
        {
            MemoryScanPattern target = new MemoryScanPattern(11, "72 0E 81 E1 FF FF 00 00 66 8B 89 ?? ?? ?? ?? C3") { OnFound = _process.ReadPointer };
            IntPtr ptr = _process.MemoryPages.Where(p => p.RegionSize == 0x10100 && p.Protect == MemoryProtection.PAGE_READWRITE)
                .Select(page => _process.Scan(target, page.BaseAddress, (int)page.RegionSize))
                .FirstOrDefault(addr => addr != IntPtr.Zero);

            if (ptr == IntPtr.Zero)
                return false;

            RamBase = ptr;
        }
        else
        {
            bool is64bit = _process.Is64Bit;

            IntPtr ptr = is64bit
                ? currentCore.ModuleName switch
                {
                    "genesis_plus_gx_libretro.dll" or "genesis_plus_gx_wide_libretro.dll" => _process.Scan(new MemoryScanPattern(3, "48 8D 0D ?? ?? ?? ?? 4C 8B 2D") { OnFound = addr => addr + 0x4 + _process.Read<int>(addr) }, currentCore),
                    "picodrive_libretro.dll" => _process.Scan(new MemoryScanPattern(3, "48 8D 0D ?? ?? ?? ?? 41 B8") { OnFound = addr => addr + 0x4 + _process.Read<int>(addr) }, currentCore),
                    _ => IntPtr.Zero,
                }
                : currentCore.ModuleName switch
                {
                    "genesis_plus_gx_libretro.dll" or "genesis_plus_gx_wide_libretro.dll" => _process.Scan(new MemoryScanPattern(1, "A3 ?? ?? ?? ?? 29 F9") { OnFound = addr => _process.ReadPointer(addr) }),
                    "picodrive_libretro.dll" => _process.Scan(new MemoryScanPattern(1, "B9 ?? ?? ?? ?? C1 EF 10") { OnFound = addr => _process.ReadPointer(addr) }),
                    _ => IntPtr.Zero,
                };

            if (ptr == IntPtr.Zero)
                return false;

            RamBase = ptr;
        }


        Log.Info($"  => RAM address found at 0x{RamBase.ToString("X")}");
        return true;
    }

    public override bool KeepAlive(ProcessMemory process)
    {
        return process.Read(core_base_address, out byte _);
    }
}