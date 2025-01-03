using System;
using Helper.Logging;
using JHelper.Common.MemoryUtils;
using JHelper.Common.ProcessInterop;

namespace Helper.Genesis.Emulators;

internal class Fusion : GenesisEmulator
{
    private IntPtr addr_base;

    internal Fusion()
        : base()
    {
        Endianness = Endianness.Big;
        Log.Info("  => Attached to emulator: Fusion");
    }

    public override bool FindRAM(ProcessMemory process)
    {
        var mainModule = process.MainModule;
        IntPtr ptr = process.Scan(new MemoryScanPattern(1, "75 2F 6A 01"), mainModule);

        if (ptr == IntPtr.Zero)
            return false;

        ptr += process.Read<byte>(ptr) + 3;

        if (!process.ReadPointer(ptr, out addr_base))
            return false;

        RamBase = process.ReadPointer(addr_base);
        if (RamBase == IntPtr.Zero)
            return false;

        Log.Info($"  => WRAM address found at 0x{RamBase.ToString("X")}");
        return true;
    }

    public override bool KeepAlive(ProcessMemory process)
    {
        if (!process.ReadPointer(addr_base, out IntPtr ptr))
            return false;

        RamBase = ptr;
        return true;
    }
}