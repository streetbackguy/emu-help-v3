using Helper.HelperBase;
using Helper.Logging;
using Helper.PS1;
using Helper.PS1.Emulators;
using System;

public class Playstation : PS1 { }

public class PlayStation : PS1 { }

public class Playstation1 : PS1 { }

public class PlayStation1 : PS1 { }

public class PS1 : HelperBase
{
    private const uint MINSIZE = 0x80000000;
    private const uint MAXSIZE = 0x80200000;

    private PS1Emulator? Ps1emulator
    {
        get => (PS1Emulator?)emulatorClass;
        set => emulatorClass = value;
    }

    public PS1()
        : this(true) { }

    public PS1(bool generateCode)
        : base(generateCode)
    {
        Log.Info("  => PS1 Helper started");
    }

    internal override string[] ProcessNames { get; } =
    [
        "ePSXe",
        "psxfin",
        "duckstation-qt-x64-ReleaseLTCG.exe",
        "duckstation-nogui-x64-ReleaseLTCG.exe",
        "retroarch.exe",
        "pcsx-redux.main",
        "XEBRA",
        "mednafen",
    ];

    public override bool TryGetRealAddress(ulong address, out IntPtr realAddress)
    {
        realAddress = default;

        if (Ps1emulator is null)
            return false;

        IntPtr baseRam = Ps1emulator.RamBase;

        if (baseRam == IntPtr.Zero)
            return false;


        if (address >= MINSIZE && address < MAXSIZE)
        {
            realAddress = (IntPtr)((ulong)baseRam + address - MINSIZE);
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
            "ePSXe" => new ePSXe(),
            "duckstation-qt-x64-ReleaseLTCG.exe" or "duckstation-nogui-x64-ReleaseLTCG.exe" => new Duckstation(),
            "mednafen" => new Mednafen(),
            "pcsx-redux.main" => new PCSXRedux(),
            "psxfin" => new pSX(),
            "retroarch.exe" => new Retroarch(),
            "XEBRA" => new Xebra(),
            _ => null,
        };
    }
}