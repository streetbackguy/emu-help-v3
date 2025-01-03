using System;
using System.Linq;
using Helper.Common.ProcessInterop;
using Helper.Logging;

namespace Helper.PS2.Emulators;

internal class Pcsx2 : PS2Emulator
{
    private IntPtr addr_base;

    internal Pcsx2()
    : base()
    {
        Log.Info("  => Attached to emulator: PCSX2");
    }

    public override bool FindRAM(ProcessMemory _process)
    {
        if (_process.Is64Bit)
        {
            addr_base = _process.Scan(new MemoryScanPattern(3, "48 8B ?? ?? ?? ?? ?? 25 F0 3F 00 00") { OnFound = addr => addr + 0x4 + _process.Read<int>(addr) });
            if (addr_base == IntPtr.Zero)
                return false;

            if (!_process.Read(addr_base, out IntPtr ptr))
                return false;
            RamBase = ptr;
        }
        else
        {
            MemoryScanPattern[] patterns =
            [
                new(2, "8B ?? ?? ?? ?? ?? 25 F0 3F 00 00") { OnFound = addr => _process.ReadPointer(addr) },
                new(2, "8B ?? ?? ?? ?? ?? 81 ?? F0 3F 00 00") { OnFound = addr => _process.ReadPointer(addr) },
            ];


            addr_base = patterns
                .Select(pattern => _process.Scan(pattern, _process.MainModule.BaseAddress, _process.MainModule.ModuleMemorySize))
                .FirstOrDefault(addr => addr != IntPtr.Zero);

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