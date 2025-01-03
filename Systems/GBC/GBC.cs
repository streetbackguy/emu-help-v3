using Helper.HelperBase;
using Helper.GBC;
using Helper.GBC.Emulators;
using System;
using Helper.Logging;

public class GameBoyColor : GBC { }

public class Gameboycolor : GBC { }

public class GameboyColor : GBC { }

public class GBC : HelperBase
{
    private const uint WRAM_MINSIZE = 0xC000;
    private const uint WRAM_MAXSIZE = 0xE000;
    private const uint IOHRAM_MINSIZE = 0xFF00;
    private const uint IOHRAM_MAXSIXE = 0x10000;

    private GBCEmulator? emulator
    {
        get => (GBCEmulator?)emulatorClass;
        set => emulatorClass = value;
    }

    public GBC()
        : this(true) { }

    public GBC(bool generateCode)
        : base(generateCode)
    {
        Log.Info("  => GBC Helper started");
    }

    internal override string[] ProcessNames { get; } =
    {
        "GSR",
        "GSE",
        "gambatte_speedrun"
    };

    public override bool TryGetRealAddress(ulong address, out IntPtr realAddress)
    {
        realAddress = default;

        if (emulator is null)
            return false;

        if (address >= WRAM_MINSIZE && address < WRAM_MAXSIZE)
        {
            realAddress = (IntPtr)((ulong)emulator.WRAM + address - WRAM_MINSIZE);
            return true;
        }
        else if (address >= IOHRAM_MINSIZE && address < IOHRAM_MAXSIXE)
        {
            realAddress = (IntPtr)((ulong)emulator.IOHRAM + address - IOHRAM_MINSIZE);
            return true;
        }
        else
        {
            return false;
        }
    }

    internal override Emulator? AttachEmuClass()
    {
        if (emulatorProcess is null)
            return null;

        return emulatorProcess.ProcessName switch
        {
            "GSR" or "GSE" => new GSR(),
            "gambatte_speedrun" => new GSR_qt(),
            _ => null,
        };
    }
}