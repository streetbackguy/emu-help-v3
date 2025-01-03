using Helper.HelperBase;
using Helper.Logging;
using Helper.SMS;
using Helper.SMS.Emulators;
using System;

public class SegaMasterSystem : SMS { }

public class SMS : HelperBase
{
    private const uint MINSIZE = 0xC000;
    private const uint MAXSIZE = 0xE000;
    private const uint MINSIZE_ALT = 0xE000;
    private const uint MAXSIZE_ALT = 0x10000;

    private SMSEmulator? emulator
    {
        get => (SMSEmulator?)emulatorClass;
        set => emulatorClass = value;
    }

    public SMS()
        : this(true) { }

    public SMS(bool generateCode)
        : base(generateCode)
    {
        Log.Info("  => SEGA Master System - Helper started");
    }

    internal override string[] ProcessNames { get; } =
    [
        "retroarch",
        "blastem",
        "Fusion",
        "mednafen",
    ];

    public override bool TryGetRealAddress(ulong address, out IntPtr realAddress)
    {
        realAddress = default;

        if (emulator is null)
            return false;

        IntPtr baseRam = emulator.RamBase;

        if (baseRam == IntPtr.Zero)
            return false;

        if (address >= MINSIZE && address < MAXSIZE)
        {
            realAddress = (IntPtr)((ulong)baseRam + address - MINSIZE);
            return true;
        }
        else if (address >= MINSIZE_ALT && address < MAXSIZE_ALT)
        {
            realAddress = (IntPtr)((ulong)baseRam + address - MINSIZE_ALT);
            return true;
        }
        return false;
    }

    internal override Emulator? AttachEmuClass()
    {
        if (emulatorProcess is null)
            return null;

        return emulatorProcess.ProcessName switch
        {
            "retroarch" => new Retroarch(),
            "blastem" => new BlastEm(),
            "Fusion" => new Fusion(),
            "mednafen" => new Mednafen(),
            _ => null,
        };
    }
}