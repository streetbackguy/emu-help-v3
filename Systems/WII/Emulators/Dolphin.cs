using System;
using System.Linq;
using EmuHelp.Logging;
using JHelper.Common.MemoryUtils;
using JHelper.Common.ProcessInterop;

namespace EmuHelp.Systems.Wii.Emulators;

internal class Dolphin : WIIEmulator
{
    internal Dolphin()
    : base()
    {
        Endianness = Endianness.Big;
        Log.Info("  => Attached to emulator: Dolphin");
    }

    public override bool FindRAM(ProcessMemory process)
    {
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
            && (long)p.BaseAddress > (long)MEM1 && (long)p.BaseAddress < (long)MEM1 + 0x10000000)
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
        return process.Read<byte>(MEM1, out _) && process.Read<byte>(MEM2, out _);
    }
}