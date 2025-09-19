using EmuHelp.HelperBase;
using EmuHelp.Logging;
using EmuHelp.Systems.SNES;
using EmuHelp.Systems.SNES.Emulators;
using System;

public class SuperNintendoEntertainmentSystem : SNES { }

public class SNES : HelperBase
{
    private const uint MINSIZE = 0x7E0000;
    private const uint MAXSIZE = 0x800000;

    private SNESEmulator? emulator
    {
        get => (SNESEmulator?)emulatorClass;
        set => emulatorClass = value;
    }

    public SNES()
#if LIVESPLIT
        : this(true) { }

    public SNES(bool generateCode)
        : base(generateCode)
#else
        : base()
#endif
    {
        Log.Info("  => SNES Helper started");
    }

    internal override string[] ProcessNames { get; } =
    [
        "retroarch.exe",
        "snes9x.exe",
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

        // Address out of range
        return false;
    }


    internal override Emulator? AttachEmuClass()
    {
        if (emulatorProcess is null)
            return null;

        return emulatorProcess.ProcessName switch
        {
            "retroarch.exe" => new Retroarch(),
            "snes9x.exe" => new Snes9x(),
            _ => null,
        };
    }
}