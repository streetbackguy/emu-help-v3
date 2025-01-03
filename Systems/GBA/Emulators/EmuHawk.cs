using System;
using System.Linq;
using Helper.Logging;
using JHelper.Common.ProcessInterop;

namespace Helper.GBA.Emulators;

internal class EmuHawk : GBAEmulator
{
    private IntPtr core_base = IntPtr.Zero;

    internal EmuHawk()
        : base()
    {
        Log.Info("  => Attached to emulator: EmuHawk");
        Log.Info("   => WARNING: This emulator is only partially supported!");
        Log.Info("   => Compatibility for this emulator can break at any time!");
    }

    public override bool FindRAM(ProcessMemory process)
    {
        if (!process.Modules.TryGetValue("mgba.dll", out ProcessModule module))
            return false;

        core_base = module.BaseAddress;

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
        return process.Read<byte>(core_base, out _) && process.Read<byte>(EWRAM, out _);
    }
}