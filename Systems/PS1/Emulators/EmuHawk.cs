using EmuHelp.Logging;
using JHelper.Common.ProcessInterop;
using System;

namespace EmuHelp.Systems.PS1.Emulators;

internal class EmuHawk : PS1Emulator
{
    private IntPtr baseInstance = IntPtr.Zero;
    private int[] offsets = { 0x20, 0x10, 0 };

    internal EmuHawk()
    : base()
    {
        Log.Info("  => Attached to emulator: EmuHawk");
    }

    public override bool FindRAM(ProcessMemory _process)
    {
        if (!Multisystems.EmuHawk.MainForm(_process, out IntPtr instance, out int emulatorOffset))
            return false;

        baseInstance = instance + emulatorOffset;

        if (!_process.ReadPointer(baseInstance, out IntPtr ptr) || ptr == IntPtr.Zero)
            RamBase = IntPtr.Zero;
        else
            RamBase = _process.DerefOffsets(baseInstance + _process.PointerSize, offsets);

        if (RamBase == IntPtr.Zero)
            Log.Info($"  => RAM address unavailable at this moment, but it will be evaluated dynamically");
        else
            Log.Info($"  => RAM address found at 0x{RamBase.ToString("X")}");

        return true;
    }

    public override bool KeepAlive(ProcessMemory process)
    {
        if (!process.ReadPointer(baseInstance, out IntPtr ptr) || ptr == IntPtr.Zero)
            RamBase = IntPtr.Zero;

        RamBase = process.DerefOffsets(baseInstance + process.PointerSize, offsets);
            return true;
    }
}