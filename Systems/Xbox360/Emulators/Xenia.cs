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

        IntPtr addr = IntPtr.Zero;

        // To identify the start of the emulated RAM, we can look for
        // the PE header of the loaded .xex in memory.
        for (int i = 32; i < 47; i++)
        {
            IntPtr tempAddr = (nint)1 << i;
            IntPtr baseModule = (IntPtr)((nint)tempAddr + 0x82000000);

            if (process.Read(baseModule, out short val) && val == 0x5A4D
                && process.Read(baseModule + 0x3C, out int e_lfanew)
                && process.Read(baseModule + e_lfanew, out int pe) && pe == 0x4550)
            { 
                addr = tempAddr;
                break;
            }
        }
        if (addr == IntPtr.Zero)
            return false;

        RamBase = addr;

        Log.Info($"  => RAM address mapped at 0x{RamBase.ToString("X")}");
        return true;
    }

    public override bool KeepAlive(ProcessMemory process)
    {
        return true;
    }
}