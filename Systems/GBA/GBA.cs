using EmuHelp.HelperBase;
using EmuHelp.Systems.GBA;
using EmuHelp.Systems.GBA.Emulators;
using System;
using EmuHelp.Logging;

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
#if LIVESPLIT
        : this(true) { }

    public GBA(bool generateCode)
        : base(generateCode)
#else
        : base()
#endif
    {
        Log.Info("  => GBA Helper started");
    }

    internal override string[] ProcessNames { get; } =
    {
        "visualboyadvance-m.exe",
        "VisualBoyAdvance.exe",
        "mGBA.exe",
        "NO$GBA.EXE",
        "retroarch.exe",
        "mednafen.exe",
        "GSR.exe",
        "GSE.exe",
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
            "visualboyadvance-m.exe" or "VisualBoyAdvance.exe" => new VisualBoyAdvance(),
            "mGBA.exe" => new mGBA(),
            "NO$GBA.EXE" => new NoCashGBA(),
            "retroarch.exe" => new Retroarch(),
            "mednafen.exe" => new Mednafen(),
            "GSR.exe" or "GSE.exe" => new GSE(),
            _ => null,
        };
    }
}