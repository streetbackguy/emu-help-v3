using System;
using System.Linq;
using Helper.Common.ProcessInterop;
using Helper.Logging;

namespace Helper.GBA.Emulators;

internal class GSE : GBAEmulator
{
    private IntPtr ewram_pointer, iwram_pointer;

    private readonly string[] ewram_symbol_name = ["GSE_GBA_EWRAM_PTR", "GSR_GBA_EWRAM_PTR"];
    private readonly string[] iwram_symbol_name = ["GSE_GBA_IWRAM_PTR", "GSR_GBA_IWRAM_PTR"];

    internal GSE()
        : base()
    {
        Log.Info("  => Attached to emulator: GSE");
    }

    public override bool FindRAM(ProcessMemory process)
    {
        var mainModule = process.MainModule;

        IntPtr ewramPtr = mainModule.Symbols.FirstOrDefault(s => ewram_symbol_name.Contains(s.Name)).Address;
        if (ewramPtr == IntPtr.Zero)
            return false;

        IntPtr iwramPtr = mainModule.Symbols.FirstOrDefault(s => iwram_symbol_name.Contains(s.Name)).Address;
        if (iwramPtr == IntPtr.Zero)
            return false;

        ewram_pointer = ewramPtr;
        iwram_pointer = iwramPtr;

        IntPtr ewram = process.ReadPointer(ewram_pointer);
        if (ewram == IntPtr.Zero)
            return false;

        IntPtr iwram = process.ReadPointer(iwram_pointer);
        if (iwram == IntPtr.Zero)
            return false;

        EWRAM = ewram;
        IWRAM = iwram;

        Log.Info($"  => EWRAM address found at 0x{EWRAM.ToString("X")}");
        Log.Info($"  => IWRAM address found at 0x{IWRAM.ToString("X")}");
        return true;
    }

    public override bool KeepAlive(ProcessMemory process)
    {
        IntPtr ewram = process.ReadPointer(ewram_pointer);
        if (ewram == IntPtr.Zero)
            return false;

        IntPtr iwram = process.ReadPointer(iwram_pointer);
        if (iwram == IntPtr.Zero)
            return false;

        EWRAM = ewram;
        IWRAM = iwram;

        return true;
    }
}