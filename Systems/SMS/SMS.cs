using EmuHelp.HelperBase;
using EmuHelp.Logging;
using EmuHelp.Systems.SMS;
using EmuHelp.Systems.SMS.Emulators;
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
#if LIVESPLIT
        : this(true) { }

    public SMS(bool generateCode)
        : base(generateCode)
#else
        : base()
#endif
    {
        Log.Info("  => SEGA Master System - Helper started");
    }

    internal override string[] ProcessNames { get; } =
    [
        "retroarch.exe",
        "blastem.exe",
        "Fusion.exe",
        "mednafen.exe",
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
            "retroarch.exe" => new Retroarch(),
            "blastem.exe" => new BlastEm(),
            "Fusion.exe" => new Fusion(),
            "mednafen.exe" => new Mednafen(),
            _ => null,
        };
    }
}