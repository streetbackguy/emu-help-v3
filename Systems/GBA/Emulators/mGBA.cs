using System;
using System.Linq;
using Helper.Common.ProcessInterop;
using Helper.Common.ProcessInterop.API;
using Helper.Logging;

namespace Helper.GBA.Emulators;

internal class mGBA : GBAEmulator
{
    internal mGBA()
        : base()
    {
        Log.Info("  => Attached to emulator: mGBA");
    }

    public override bool FindRAM(ProcessMemory process)
    {
        IntPtr ptr = process.MemoryPages
        .FirstOrDefault(p => (int)p.RegionSize == 0x48000
            && p.Type == MemoryType.MEM_PRIVATE
            && p.State == MemoryState.MEM_COMMIT
            && p.Protect == MemoryProtection.PAGE_READWRITE)
        .BaseAddress;

        if (ptr == IntPtr.Zero)
            return false;

        EWRAM = ptr;
        IWRAM = ptr + 0x40000;

        Log.Info($"  => EWRAM address found at 0x{EWRAM.ToString("X")}");
        Log.Info($"  => IWRAM address found at 0x{IWRAM.ToString("X")}");
        return true;
    }

    public override bool KeepAlive(ProcessMemory process)
    {
        return process.Read<byte>(EWRAM, out _);
    }
}