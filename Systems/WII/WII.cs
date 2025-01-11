using EmuHelp.HelperBase;
using EmuHelp.Logging;
using EmuHelp.Systems.Wii;
using EmuHelp.Systems.Wii.Emulators;
using JHelper.Common.MemoryUtils;
using System;
using System.Runtime.InteropServices;

public class Wii : WII { }

public class WII : HelperBase
{
    private const uint MEM1_MINSIZE = 0x80000000;
    private const uint MEM1_MAXSIZE = 0x81800000;
    private const uint MEM2_MINSIZE = 0x90000000;
    private const uint MEM2_MAXSIZE = 0x94000000;

    private WIIEmulator? Wiiemulator
    {
        get => (WIIEmulator?)emulatorClass;
        set => emulatorClass = value;
    }

    public WII()
#if LIVESPLIT
        : this(true) { }

    public WII(bool generateCode)
        : base(generateCode)
#else
        : base()
#endif
    {
        Log.Info("  => Wii Helper started");
    }

    internal override string[] ProcessNames { get; } =
    [
        "Dolphin.exe",
        "retroarch.exe",
    ];

    public override bool TryGetRealAddress(ulong address, out IntPtr realAddress)
    {
        if (Wiiemulator is null)
        {
            realAddress = default;
            return false;
        }

        if (address >= MEM1_MINSIZE && address < MEM1_MAXSIZE)
        {
            realAddress = (IntPtr)((ulong)Wiiemulator.MEM1 + address - MEM1_MINSIZE);
            return true;
        }
        else if (address >= MEM2_MINSIZE && address < MEM2_MAXSIZE)
        {
            realAddress = (IntPtr)((ulong)Wiiemulator.MEM2 + address - MEM2_MINSIZE);
            return true;
        }
        else
        {
            realAddress = default;
            return false;
        }
    }

    internal override Emulator? AttachEmuClass()
    {
        if (emulatorProcess is null)
            return null;

        return emulatorProcess.ProcessName switch
        {
            "Dolphin.exe" => new Dolphin(),
            "retroarch.exe" => new Retroarch(),
            _ => null,
        };
    }

    public override bool TryRead<T>(out T value, ulong address, params int[] offsets)
    {
        value = default;

        if (emulatorProcess is null || Wiiemulator is null || !ResolvePath(out IntPtr realAddress, address, offsets))
            return false;

        int t_size;
        unsafe
        {
            t_size = sizeof(T);
        }

        using (ArrayRental<byte> buffer = t_size <= 1024 ? new(stackalloc byte[t_size]) : new(t_size))
        {
            Span<byte> span = buffer.Span;

            if (!emulatorProcess.ReadArray(realAddress, span))
                return false;
            if (Wiiemulator.Endianness == Endianness.Big)
                span.Reverse();

            unsafe
            {
                fixed (byte* ptr = span)
                {
                    value = *(T*)ptr;
                }
            }
        }
        return true;
    }

    protected override bool ResolvePath(out IntPtr finalAddress, ulong baseAddress, params int[] offsets)
    {
        // Check if the emulator process is valid and retrieve the real address for the base address
        if (emulatorProcess is null || Wiiemulator is null || !TryGetRealAddress(baseAddress, out finalAddress))
        {
            finalAddress = default;
            return false;
        }

        foreach (int offset in offsets)
        {
            if (!emulatorProcess.Read(finalAddress, out uint tempAddress)
                || !TryGetRealAddress((ulong)(tempAddress.FromEndian(Wiiemulator.Endianness) + offset), out finalAddress))
                return false;
        }

        return true;
    }

    public override bool TryReadArray<T>(out T[] value, uint size, ulong address, params int[] offsets)
    {
        if (emulatorProcess is null || Wiiemulator is null || !ResolvePath(out IntPtr realAddress, address, offsets))
        {
            value = new T[size];
            return false;
        }

        int t_size;
        unsafe
        {
            t_size = sizeof(T) * (int)size;
        }

        using (ArrayRental<byte> buffer = t_size <= 1024 ? new(stackalloc byte[t_size]) : new(t_size))
        {
            Span<byte> span = buffer.Span;

            if(!emulatorProcess.ReadArray(realAddress, span))
            {
                value = new T[size];
                return false;
            }

            if (Wiiemulator.Endianness == Endianness.Big)
            {
                int s = Marshal.SizeOf<T>();
                for (int i = 0; i < size; i++)
                    span[(s * i)..(s * (i + 1))].Reverse();
            }

            Span<T> newBuf = MemoryMarshal.Cast<byte, T>(span);
            value = newBuf[..(int)size].ToArray();
        }
        return true;
    }
}