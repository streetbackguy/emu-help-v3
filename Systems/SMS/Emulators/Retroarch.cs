using System;
using System.Linq;
using Helper.Logging;
using JHelper.Common.ProcessInterop;

namespace Helper.SMS.Emulators;

internal class Retroarch : SMSEmulator
{
    private IntPtr core_base_address;

    private readonly string[] supportedCores =
    [
        "genesis_plus_gx_libretro.dll",
        "genesis_plus_gx_wide_libretro.dll",
        "picodrive_libretro.dll",
        "smsplus_libretro.dll",
        "gearsystem_libretro.dll",
    ];

    internal Retroarch()
        : base()
    {
        Log.Info("  => Attached to emulator: Retroarch");
    }

    public override bool FindRAM(ProcessMemory _process)
    {
        ProcessModule currentCore = _process.Modules.FirstOrDefault(m => supportedCores.Contains(m.ModuleName));
        if (currentCore is null)
            return false;

        core_base_address = currentCore.BaseAddress;

        IntPtr ptr = _process.Is64Bit
            ? currentCore.ModuleName switch
            {
                "genesis_plus_gx_libretro.dll" or "genesis_plus_gx_wide_libretro.dll" => _process.Scan(new MemoryScanPattern(3, "48 8D 0D ?? ?? ?? ?? 4C 8B 2D") { OnFound = addr => addr + 0x4 + _process.Read<int>(addr) }, currentCore),
                "picodrive_libretro.dll" => _process.Scan(new MemoryScanPattern(3, "48 8D 0D ?? ?? ?? ?? 41 B8") { OnFound = addr => addr + 0x4 + _process.Read<int>(addr) }, currentCore),
                "smsplus_libretro.dll" => _process.Scan(new MemoryScanPattern(5, "31 F6 48 C7 05") { OnFound = addr => addr + 0x8 + _process.Read<int>(addr) }, currentCore),
                "gearsystem_libretro.dll" => _process.Scan(new MemoryScanPattern(8, "83 ?? 02 75 ?? 48 8B 0D ?? ?? ?? ?? E8") { OnFound = addr =>
                {
                    byte offset = _process.Read<byte>(addr + 13 + 0x4 + _process.Read<int>(addr + 13) + 0x3);
                    IntPtr ptr = (IntPtr)_process.Read<long>(addr + 0x4 + _process.Read<int>(addr));

                    foreach (var entry in new int[] { 0, 0, offset })
                    {
                        ptr = (IntPtr)_process.Read<long>(ptr + entry);
                        if (ptr == IntPtr.Zero)
                            return IntPtr.Zero;
                    }
                    return ptr + 0xC000;
                } }, currentCore),
                _ => IntPtr.Zero,
            }
            : currentCore.ModuleName switch
            {
                "genesis_plus_gx_libretro.dll" or "genesis_plus_gx_wide_libretro.dll" => _process.Scan(new MemoryScanPattern(1, "A3 ?? ?? ?? ?? 29 F9") { OnFound = addr => (IntPtr)_process.Read<int>(addr) }, currentCore),
                "picodrive_libretro.dll" => _process.Scan(new MemoryScanPattern(1, "B9 ?? ?? ?? ?? C1 EF 10") { OnFound = addr => (IntPtr)_process.Read<int>(addr) + 0x20000 }, currentCore),
                "smsplus_libretro.dll" => _process.Scan(new MemoryScanPattern(4, "83 FA 02 B8") { OnFound = addr => (IntPtr)_process.Read<int>(addr) }, currentCore),
                "gearsystem_libretro.dll" => _process.Scan(new MemoryScanPattern(7, "83 ?? 02 75 ?? 8B ?? ?? ?? ?? ?? E8") { OnFound = addr =>
                {
                    byte offset = _process.Read<byte>(addr + 12 + 0x4 + _process.Read<int>(addr + 12) + 0x2);
                    IntPtr ptr = (IntPtr)_process.Read<int>(addr);

                    foreach (var entry in new int[] { 0, 0, offset })
                    {
                        ptr = (IntPtr)_process.Read<int>(ptr + entry);
                        if (ptr == IntPtr.Zero)
                            return IntPtr.Zero;
                    }
                    return ptr + 0xC000;
                } }, currentCore),
                _ => IntPtr.Zero,
            };

        if (ptr == IntPtr.Zero)
            return false;
        RamBase = ptr;

        Log.Info($"  => RAM address found at 0x{RamBase.ToString("X")}");
        return true;
    }

    public override bool KeepAlive(ProcessMemory process)
    {
        return process.Read<byte>(core_base_address, out _);
    }
}