using System;
using System.Linq;
using Helper.Logging;
using JHelper.Common.ProcessInterop;

namespace Helper.GBA.Emulators;

internal class VisualBoyAdvance : GBAEmulator
{
    private IntPtr ewram_pointer;
    private IntPtr iwram_pointer;
    private IntPtr is_emulating;

    internal VisualBoyAdvance()
        : base()
    {
        Log.Info("  => Attached to emulator: VisualBoyAdvance");
    }

    public override bool FindRAM(ProcessMemory process)
    {
        var mainModule = process.MainModule;

        if (process.Is64Bit)
        {
            ewram_pointer = process.Scan(new MemoryScanPattern(3, "48 8B 05 ?? ?? ?? ?? 81 E3 FF FF 03 00")
            { OnFound = addr => { var ptr = addr + 0x4 + process.Read<int>(addr); if (process.Read<byte>(addr + 10) == 0x48) ptr = process.ReadPointer(ptr); return ptr; } });
            if (ewram_pointer == IntPtr.Zero)
                return false;

            iwram_pointer = process.Scan(new MemoryScanPattern(3, "48 8B 05 ?? ?? ?? ?? 81 E3 FF 7F 00 00")
            { OnFound = addr => { var ptr = addr + 0x4 + process.Read<int>(addr); if (process.Read<byte>(addr + 10) == 0x48) ptr = process.ReadPointer(ptr); return ptr; } });
            if (iwram_pointer == IntPtr.Zero)
                return false;

            MemoryScanPattern[] isEmulating =
            [
                new(2, "83 3D ?? ?? ?? ?? 00 74 ?? 80 3D ?? ?? ?? ?? 00 75 ?? 66") { OnFound = addr => addr + 0x4 + process.Read<int>(addr) + 0x1 },
                new(3, "48 8B 15 ?? ?? ?? ?? 31 C0 8B 12 85 D2 74 ?? 48") { OnFound = addr => process.ReadPointer(addr + 0x4 + process.Read<int>(addr)) },
            ];

            is_emulating = isEmulating
                .Select(val => process.Scan(val, mainModule.BaseAddress, mainModule.ModuleMemorySize))
                .FirstOrDefault(addr => addr != IntPtr.Zero);

            if (is_emulating == IntPtr.Zero)
                return false;
        }
        else
        {
            ewram_pointer = process.Scan(new MemoryScanPattern(1, "A1 ?? ?? ?? ?? 81 ?? FF FF 03 00") { OnFound = addr => process.ReadPointer(addr) });
            if (ewram_pointer == IntPtr.Zero)
                return false;

            if (ewram_pointer != IntPtr.Zero)
            {
                iwram_pointer = process.Scan(new MemoryScanPattern(1, "A1 ?? ?? ?? ?? 81 ?? FF 7F 00 00") { OnFound = process.ReadPointer });
                if (iwram_pointer == IntPtr.Zero)
                    return false;

                MemoryScanPattern[] isEmulating =
                [
                    new(2, "83 3D ?? ?? ?? ?? 00 74 ?? 80 3D ?? ?? ?? ?? 00 75 ?? 66") { OnFound = process.ReadPointer },
                    new(2, "8B 15 ?? ?? ?? ?? 31 C0 85 D2 74 ?? 0F") { OnFound = process.ReadPointer },
                ];

                foreach (var entry in isEmulating)
                {
                    is_emulating = process.Scan(entry);
                    if (is_emulating != IntPtr.Zero)
                        break;
                }
                if (is_emulating == IntPtr.Zero)
                    return false;
            }
            else
            {
                ewram_pointer = process.Scan(new MemoryScanPattern(8, "81 E6 FF FF 03 00 8B 15") { OnFound = process.ReadPointer });
                if (ewram_pointer == IntPtr.Zero)
                    return false;
                iwram_pointer = ewram_pointer + 0x4;

                is_emulating = process.Scan(new MemoryScanPattern(2, "8B 0D ?? ?? ?? ?? 85 C9 74 ?? 8A") { OnFound = process.ReadPointer });
                if (is_emulating == IntPtr.Zero)
                    return false;
            }
        }

        EWRAM = process.ReadPointer(ewram_pointer);
        IWRAM = process.ReadPointer(ewram_pointer);

        Log.Info(EWRAM != IntPtr.Zero ? $"  => EWRAM address found at 0x{EWRAM.ToString("X")}" : "  => EWRAM address will be evaluated while emulator is running");
        Log.Info(IWRAM != IntPtr.Zero ? $"  => IWRAM address found at 0x{IWRAM.ToString("X")}" : "  => IWRAM address will be evaluated while emulator is running");
        return true;
    }

    public override bool KeepAlive(ProcessMemory _process)
    {
        if (!_process.Read(is_emulating, out bool isok))
            return false;

        EWRAM = !isok ? IntPtr.Zero : _process.ReadPointer(ewram_pointer);
        IWRAM = !isok ? IntPtr.Zero : _process.ReadPointer(ewram_pointer);
        return true;
    }
}