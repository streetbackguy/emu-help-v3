using System;
using Helper.Common.ProcessInterop;
using Helper.Common.ProcessInterop.API;
using Helper.Logging;

namespace Helper.PS1.Emulators;

internal class PCSXRedux : PS1Emulator
{
    private IntPtr addr_base, addr;

    internal PCSXRedux()
    : base()
    {
        Log.Info("  => Attached to emulator: PCSX Redux");
    }

    public override bool FindRAM(ProcessMemory _process)
    {
        if (_process.Is64Bit)
        {
            addr_base = _process.Scan(new ScanPattern(2, "48 B9 ?? ?? ?? ?? ?? ?? ?? ?? E8 ?? ?? ?? ?? C7 85 ?? ?? ?? ?? 00 00 00 00"));
            if (addr_base == IntPtr.Zero)
                return false;

            if (!_process.Read(addr_base, out IntPtr ptrl))
                return false;
            addr = ptrl;

            IntPtr ptr = _process.Scan(new ScanPattern(8, "89 D1 C1 E9 10 48 8B"));
            if (ptr == IntPtr.Zero)
                return false;

            if (!_process.Read<byte>(ptr, out byte offset))
                return false;

            if (!_process.Read(addr + offset, out ptrl))
                return false;
            ptr = ptrl;

            if (!_process.Read(ptr, out ptrl))
                return false;

            RamBase = ptrl;
        }
        else
        {
            ScanPattern target = new(2, "8B 3D 20 ?? ?? ?? 0F B7 D3 8B 04 95 ?? ?? ?? ?? 21 05");

            foreach (MemoryPage page in _process.MemoryPages)
            {
                addr_base = _process.Scan(target, page.BaseAddress, (int)page.RegionSize);

                if (addr_base != IntPtr.Zero)
                    break;
            }
            if (addr_base == IntPtr.Zero)
                return false;

            if (!_process.Read<int>(addr_base, out int ptrl))
                return false;

            addr = (IntPtr)ptrl;
            RamBase = addr;
        }

        Log.Info($"  => RAM address found at 0x{RamBase.ToString("X")}");
        return true;
    }

    public override bool KeepAlive(ProcessMemory process)
    {
        return process.Read(addr_base, out IntPtr ptr) && ptr == addr;
    }
}