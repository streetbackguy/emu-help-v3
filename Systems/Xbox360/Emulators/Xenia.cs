using System;
using EmuHelp.Logging;
using JHelper.Common.MemoryUtils;
using JHelper.Common.ProcessInterop;

namespace EmuHelp.Systems.Xbox360.Emulators;

internal class Xenia : Xbox360Emulator
{
    internal Xenia()
        : base()
    {
        Endianness = Endianness.Big;
        Log.Info("  => Attached to emulator: Xenia");
    }

    public override bool FindRAM(ProcessMemory process)
    {
        if (!process.Is64Bit)
            return false;

        RamBase = (IntPtr)0x100000000;

        Log.Info($"  => RAM address mapped at 0x{RamBase.ToString("X")}");
        return true;
    }

    public override bool KeepAlive(ProcessMemory process)
    {
        return true;
    }
}