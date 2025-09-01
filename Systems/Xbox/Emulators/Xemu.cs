using EmuHelp.Logging;
using JHelper.Common.MemoryUtils;
using JHelper.Common.ProcessInterop;
using System;
using System.Linq;

namespace EmuHelp.Systems.Xbox.Emulators;

internal class Xemu : XboxEmulator
{
    internal Xemu()
        : base()
    {
        Endianness = Endianness.Big;
        Log.Info("  => Attached to emulator: Xemu");
    }

    public override bool FindRAM(ProcessMemory process)
    {
        if (!process.Is64Bit)
            return false;

        IntPtr ramBaseCandidate = IntPtr.Zero;

        long maxAddress = 0x7FFFFFFFFFFF;
        long step = 0x100000;
        const int checkSize = 1024;
        const double zeroThreshold = 0.9;

        for (long addr = 0; addr < maxAddress; addr += step)
        {
            IntPtr testAddr = new IntPtr(addr);
            try
            {
                byte[] data = new byte[checkSize];
                int zeroCount = 0;
                bool readable = true;

                for (int i = 0; i < checkSize; i++)
                {
                    if (!process.Read(new IntPtr(testAddr.ToInt64() + i), out byte value))
                    {
                        readable = false;
                        break;
                    }
                    if (value == 0) zeroCount++;
                }

                if (!readable)
                    continue;

                double zeroRatio = (double)zeroCount / checkSize;
                if (zeroRatio >= zeroThreshold)
                {
                    IntPtr writeTestAddr = new IntPtr(testAddr.ToInt64() + 0x1000);
                    if (process.Read(writeTestAddr, out int original))
                    {
                        try { process.Write(writeTestAddr, original); }
                        catch { continue; }
                    }

                    ramBaseCandidate = testAddr;
                    break;
                }
            }
            catch
            {
                continue;
            }
        }

        if (ramBaseCandidate == IntPtr.Zero)
        {
            Log.Info("  => Could not find RAM base dynamically.");
            return false;
        }

        RamBase = ramBaseCandidate;
        Log.Info($"  => RAM address mapped at 0x{RamBase.ToString("X")}");
        return true;
    }

    public override bool KeepAlive(ProcessMemory process)
    {
        return true;
    }
}