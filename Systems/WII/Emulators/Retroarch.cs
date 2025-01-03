using System;
using System.Linq;
using Helper.Logging;
using JHelper.Common.MemoryUtils;
using JHelper.Common.ProcessInterop;

namespace Helper.Wii.Emulators;

internal class Retroarch : WIIEmulator
{
    private IntPtr core_base_address;
    private readonly string[] supportedCores = [ "dolphn_libretro.dll" ];

    internal Retroarch()
        : base()
    {
        Endianness = Endianness.Big;
        Log.Info("  => Attached to emulator: Retroarch");
    }

    public override bool FindRAM(ProcessMemory process)
    {
        if (!process.Is64Bit)
            return false;

        var currentCore = process.Modules.FirstOrDefault(m => supportedCores.Contains(m.ModuleName));
        if (currentCore == null)
            return false;

        core_base_address = currentCore.BaseAddress;

        IntPtr ptr = process.MemoryPages
            .FirstOrDefault(p => p.Type == MemoryType.MEM_MAPPED
            && (int)p.RegionSize == 0x2000000
            && process.Read<long>(p.BaseAddress + 0x3118) == 0x0000000400000004)
            .BaseAddress;

        if (ptr == IntPtr.Zero)
            return false;

        MEM1 = ptr;

        ptr = process.MemoryPages
            .FirstOrDefault(p => p.Type == MemoryType.MEM_MAPPED
            && (int)p.RegionSize == 0x4000000
            && (nint)p.BaseAddress > (nint)MEM1 && (nint)p.BaseAddress < (nint)MEM1 + 0x10000000)
            .BaseAddress;

        if (ptr == IntPtr.Zero)
            return false;

        MEM2 = ptr;

        Log.Info($"  => MEM1 address found at 0x{MEM1.ToString("X")}");
        Log.Info($"  => MEM2 address found at 0x{MEM2.ToString("X")}");
        return true;
    }

    public override bool KeepAlive(ProcessMemory process)
    {
        return process.Read(core_base_address, out byte _);
    }
}