using EmuHelp.Logging;
using JHelper.Common.ProcessInterop;
using System;
using System.Linq;

namespace EmuHelp.Systems.PS2.Emulators;

internal class Retroarch : PS2Emulator
{
    private IntPtr core_base_address;
    private readonly string[] supportedCores =
        {
                "pcsx2_libretro.dll",
        };

    internal Retroarch()
    : base()
    {
        Log.Info("  => Attached to emulator: Retroarch");
    }

    public override bool FindRAM(ProcessMemory _process)
    {
        if (!_process.Is64Bit)
            return false;

        ProcessModule currentCore = _process.Modules.FirstOrDefault(m => supportedCores.Contains(m.ModuleName));
        if (currentCore == default)
            return false;

        core_base_address = currentCore.BaseAddress;

        IntPtr ptr = _process.Scan(new MemoryScanPattern(3, "48 8B ?? ?? ?? ?? ?? 81 ?? F0 3F 00 00") { OnFound = addr => addr + 0x4 + _process.Read<int>(addr) }, currentCore);
        if (ptr == IntPtr.Zero)
            return false;

        RamBase = ptr;

        Log.Info($"  => RAM address found at 0x{RamBase.ToString("X")}");
        return true;
    }

    public override bool KeepAlive(ProcessMemory process)
    {
        return process.Read(core_base_address, out byte _);
    }
}