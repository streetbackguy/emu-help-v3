using System;
using EmuHelp.Logging;
using JHelper.Common.ProcessInterop;

namespace EmuHelp.Systems.PS1.Emulators;

internal class PCSXRedux : PS1Emulator
{
    private IntPtr baseAddress;
    private int[]? offsets;

    internal PCSXRedux()
        : base()
    {
        Log.Info("  => Attached to emulator: PCSX Redux");
    }

    public override bool FindRAM(ProcessMemory _process)
    {
        if (!_process.Is64Bit)
            return false;

        baseAddress = _process.Scan(new MemoryScanPattern(3, "48 8B 0D ?? ?? ?? ?? ?? ?? ?? FF FF FF 00") { OnFound = addr => addr + 0x4 + _process.Read<int>(addr) });
        // baseAddress = _process.Scan(new MemoryScanPattern(3, "48 8B 05 ?? ?? ?? ?? 45 85 C0") { OnFound = addr => addr + 0x4 + _process.Read<int>(addr) });
        if (baseAddress == IntPtr.Zero)
            return false;

        var pAddr = _process.Scan(new MemoryScanPattern(0, "4C 8B 99 ?? ?? ?? ?? 4D 8B 7B"));
        if (pAddr == IntPtr.Zero)
            return false;

        offsets = [ _process.Read<int>(pAddr + 3), _process.Read<byte>(pAddr + 10), 0 ];
        RamBase = _process.DerefOffsets(baseAddress, offsets);

        if (RamBase != IntPtr.Zero)
            Log.Info($"  => RAM address found at 0x{RamBase.ToString("X")}");

        return true;
    }

    public override bool KeepAlive(ProcessMemory process)
    {
        if (offsets is null)
            return false;

        RamBase = process.DerefOffsets(baseAddress, offsets);
        return true;
    }
}