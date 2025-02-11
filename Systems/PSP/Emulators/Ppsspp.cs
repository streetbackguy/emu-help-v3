using System;
using System.Linq;
using EmuHelp.Logging;
using JHelper.Common.ProcessInterop;

namespace EmuHelp.Systems.PSP.Emulators;

internal class Ppsspp : PSPEmulator
{
    private IntPtr addr_base;

    internal Ppsspp()
        : base()
    {
        Log.Info("  => Attached to emulator: PPSSPP");
    }

    public override bool FindRAM(ProcessMemory _process)
    {
        if (_process.Is64Bit)
        {
            addr_base = _process.Scan(new MemoryScanPattern(22, "41 B9 ?? 05 00 00 48 89 44 24 20 8D 4A FC E8 ?? ?? ?? FF 48 8B 0D ?? ?? ?? 00 48 03 CB") { OnFound = addr => addr + 0x4 + _process.Read<int>(addr) });
            if (addr_base == IntPtr.Zero)
                return false;

            if (!_process.Read(addr_base, out IntPtr ptr))
                return false;
            RamBase = ptr;
        }
        else
        {
            addr_base = new MemoryScanPattern[]
            {
                new(2, "8B ?? ?? ?? ?? ?? 25 F0 3F 00 00") { OnFound = _process.ReadPointer },
                new(2, "8B ?? ?? ?? ?? ?? 81 ?? F0 3F 00 00") { OnFound = _process.ReadPointer },
            }
            .Select(_process.Scan).FirstOrDefault(addr => addr != IntPtr.Zero);

            if (addr_base == IntPtr.Zero)
                return false;

            if (!_process.Read(addr_base, out IntPtr ptr))
                return false;
            RamBase = ptr;
        }

        if (RamBase != IntPtr.Zero)
            Log.Info($"  => RAM address found at 0x{RamBase.ToString("X")}");
        return true;
    }

    public override bool KeepAlive(ProcessMemory process)
    {
        if (!process.Read(addr_base, out IntPtr ptr))
            return false;
        RamBase = ptr;

        return true;
    }
}
