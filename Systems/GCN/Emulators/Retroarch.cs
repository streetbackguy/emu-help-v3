using System;
using System.Linq;
using EmuHelp.Logging;
using JHelper.Common.MemoryUtils;
using JHelper.Common.ProcessInterop;

namespace EmuHelp.Systems.GCN.Emulators;

internal class Retroarch : GCNEmulator
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
            .FirstOrDefault(p => p.Type == MemoryType.MEM_MAPPED && (int)p.RegionSize == 0x2000000 && process.Read<uint>(p.BaseAddress + 0x1C, out uint value) && value == 0x3D9F33C2)
            .BaseAddress;

        if (ptr == IntPtr.Zero)
            return false;

        MEM1 = ptr;

        Log.Info($"  => RAM address found at 0x{MEM1.ToString("X")}");
        return true;
    }

    public override bool KeepAlive(ProcessMemory process)
    {
        return process.Read(core_base_address, out byte _);
    }
}