using Helper.HelperBase;
using Helper.GBA;
using Helper.GBA.Emulators;
using System;
using Helper.Logging;

public class Gameboyadvance : GBA { }

public class GameBoyAdvance : GBA { }

public class GameboyAdvance : GBA { }

public class GBA : HelperBase
{
    private const uint EWRAM_MINSIZE = 0x02000000;
    private const uint EWRAM_MAXSIZE = 0x02040000;
    private const uint IWRAM_MINSIZE = 0x03000000;
    private const uint IWRAM_MAXSIXE = 0x03008000;

    private GBAEmulator? emulator
    {
        get => (GBAEmulator?)emulatorClass;
        set => emulatorClass = value;
    }

    public GBA()
        : this(true) { }

    public GBA(bool generateCode)
        : base(generateCode)
    {
        Log.Info("  => GBA Helper started");
    }

    internal override string[] ProcessNames { get; } =
    {
        "visualboyadvance-m",
        "VisualBoyAdvance",
        "mGBA",
        "NO$GBA",
        "retroarch",
        "EmuHawk",
        "mednafen",
        "GSR",
        "GSE",
    };

    public override bool TryGetRealAddress(ulong address, out IntPtr realAddress)
    {
        realAddress = default;

        if (emulator is null)
            return false;

        if (address >= EWRAM_MINSIZE && address < EWRAM_MAXSIZE && emulator.EWRAM != IntPtr.Zero)
        {
            realAddress = (IntPtr)((ulong)emulator.EWRAM + address - EWRAM_MINSIZE);
            return true;
        }
        else if (address >= IWRAM_MINSIZE && address < IWRAM_MAXSIXE && emulator.IWRAM != IntPtr.Zero)
        {
            realAddress = (IntPtr)((ulong)emulator.IWRAM + address - IWRAM_MINSIZE);
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
            "visualboyadvance-m" or "VisualBoyAdvance" => new VisualBoyAdvance(),
            "mGBA" => new mGBA(),
            "NO$GBA" => new NoCashGBA(),
            "retroarch" => new Retroarch(),
            "EmuHawk" => new EmuHawk(),
            "mednafen" => new Mednafen(),
            "GSR" or "GSE" => new GSE(),
            _ => null,
        };
    }
}