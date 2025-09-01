using EmuHelp.HelperBase;
using EmuHelp.Logging;
using EmuHelp.Systems.Xbox;
using EmuHelp.Systems.Xbox.Emulators;
using JHelper.Common.MemoryUtils;
using System;
using System.Runtime.InteropServices;

public class X : Xbox { }

public class Xbox : HelperBase
{
    // Xbox 360s Memory Regions
    // 0x30000000 - Heap Memory Region
    // 0x40000000 - Allocated Data Memory Region
    // 0x70000000 - the Stack Memory Region
    // 0x82000000 - Basefiles Memory Region
    // 0xC0000000 – 0xDFFFFFFF - Full 512mb Ram Memory Region

    private XboxEmulator? Xboxemulator
    {
        get => (XboxEmulator?)emulatorClass;
        set => emulatorClass = value;
    }

    public Xbox()
#if LIVESPLIT
        : this(true) { }

    public Xbox(bool generateCode)
        : base(generateCode)
#else
        : base()
#endif
    {
        Log.Info("  => Xbox Helper started");
    }

    internal override string[] ProcessNames { get; } =
    [
        "xemu.exe",
    ];

    public override bool TryGetRealAddress(ulong address, out IntPtr realAddress)
    {
        if (Xboxemulator is null)
        {
            realAddress = default;
            return false;
        }

        realAddress = (IntPtr)((ulong)Xboxemulator.RamBase + address);
        return true;
    }

    internal override Emulator? AttachEmuClass()
    {
        if (emulatorProcess is null)
            return null;

        return emulatorProcess.ProcessName switch
        {
            "xemu.exe" => new Xemu(),
            _ => null,
        };
    }

    public override bool TryRead<T>(out T value, ulong address, params int[] offsets)
    {
        if (emulatorProcess is null || Xboxemulator is null || !ResolvePath(out IntPtr realAddress, address, offsets))
        {
            value = default;
            return false;
        }

        return Xboxemulator.Endianness == Endianness.Big
            ? emulatorProcess.ReadBigEndian(realAddress, out value)
            : emulatorProcess.Read(realAddress, out value);
    }

    protected override bool ResolvePath(out IntPtr finalAddress, ulong baseAddress, params int[] offsets)
    {
        // Check if the emulator process is valid and retrieve the real address for the base address
        if (emulatorProcess is null || Xboxemulator is null || !TryGetRealAddress(baseAddress, out finalAddress))
        {
            finalAddress = default;
            return false;
        }

        foreach (int offset in offsets)
        {
            uint tempAddress;

            if (!(Xboxemulator.Endianness == Endianness.Big
                ? emulatorProcess.ReadBigEndian(finalAddress, out tempAddress)
                : emulatorProcess.Read(finalAddress, out tempAddress))
                || !TryGetRealAddress((ulong)(tempAddress + offset), out finalAddress))
                return false;
        }

        return true;
    }

    public override unsafe bool TryReadArray<T>(out T[] value, uint size, ulong address, params int[] offsets)
    {
        if (emulatorProcess is null || Xboxemulator is null || !ResolvePath(out IntPtr realAddress, address, offsets))
        {
            value = new T[size];
            return false;
        }

        using (ArrayRental<T> buffer = (int)size * sizeof(T) <= 1024 ? new(stackalloc T[(int)size]) : new((int)size))
        {
            if (!(Xboxemulator.Endianness == Endianness.Big
                ? emulatorProcess.ReadArrayBigEndian(realAddress, buffer.Span)
                : emulatorProcess.ReadArray(realAddress, buffer.Span)))
            {
                value = new T[(int)size];
                return false;
            }

            value = buffer.Span.ToArray();
        }

        return true;
    }
}