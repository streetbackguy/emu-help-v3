using System;
using System.Linq;
using Helper.Common.ProcessInterop;
using Helper.Common.MemoryUtils;
using Helper.Logging;

namespace Helper.GCN.Emulators;

internal class Dolphin : GCNEmulator
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
            .FirstOrDefault(p => p.Type == MemoryType.MEM_MAPPED && p.RegionSize == 0x2000000 && process.Read<uint>(p.BaseAddress + 0x1C, out uint value) && value == 0x3D9F33C2)
            .BaseAddress;

        if (ptr == IntPtr.Zero)
            return false;

        MEM1 = ptr;

        Log.Info($"  => MEM1 address found at 0x{MEM1.ToString("X")}");
        return true;
    }

    public override bool KeepAlive(ProcessMemory process)
    {
        return process.Read<byte>(MEM1, out _);
    }
}