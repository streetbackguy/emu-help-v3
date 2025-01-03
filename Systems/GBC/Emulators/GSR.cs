using System;
using System.Linq;
using Helper.Logging;
using JHelper.Common.ProcessInterop;

namespace Helper.GBC.Emulators;

internal class GSR : GBCEmulator
{
    private IntPtr wram_pointer, iohram_pointer;

    private readonly string[] wram_symbol_name = ["GSE_GB_WRAM_PTR", "GSR_GB_WRAM_PTR"];
    private readonly string[] iohram_symbol_name = ["GSE_GB_HRAM_PTR", "GSR_GB_HRAM_PTR"];

    internal GSR()
        : base()
    {
        Log.Info("  => Attached to emulator: GSR");
    }

    public override bool FindRAM(ProcessMemory process)
    {
        var mainModule = process.MainModule;

        IntPtr ewramPtr = mainModule.Symbols.FirstOrDefault(s => wram_symbol_name.Contains(s.Name)).Address;
        if (ewramPtr == IntPtr.Zero)
            return false;

        IntPtr iohramPtr = mainModule.Symbols.FirstOrDefault(s => iohram_symbol_name.Contains(s.Name)).Address;
        if (iohramPtr == IntPtr.Zero)
            return false;

        wram_pointer = ewramPtr;
        iohram_pointer = iohramPtr;

        if (!process.ReadPointer(wram_pointer, out IntPtr wram) || !process.ReadPointer(iohramPtr, out IntPtr iohram))
            return false;
        WRAM = wram;
        IOHRAM = iohram - 0x80;

        Log.Info($"  => WRAM address found at 0x{WRAM.ToString("X")}");
        Log.Info($"  => IO_HRAM address found at 0x{IOHRAM.ToString("X")}");
        return true;
    }

    public override bool KeepAlive(ProcessMemory process)
    {
        if (!process.ReadPointer(wram_pointer, out IntPtr wram) || !process.ReadPointer(iohram_pointer, out IntPtr iohram))
            return false;
        WRAM = wram;
        IOHRAM = iohram - 0x80;

        return true;
    }
}