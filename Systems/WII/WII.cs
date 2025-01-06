using Helper.HelperBase;
using Helper.Logging;
using Helper.Wii;
using Helper.Wii.Emulators;
using JHelper.Common.MemoryUtils;
using System;
using System.Buffers;
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
        "Dolphin",
        "retroarch",
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
            "Dolphin" => new Dolphin(),
            "retroarch" => new Retroarch(),
            _ => null,
        };
    }

    public override bool TryRead<T>(out T value, ulong address, params int[] offsets)
    {
        value = default;

        if (emulatorProcess is null || Wiiemulator is null || !ResolvePath(out IntPtr realAddress, address, offsets))
            return false;

        int t_size = Marshal.SizeOf<T>();

        byte[]? rented = null;
        Span<byte> buffer = t_size <= 1024
            ? stackalloc byte[t_size]
            : (rented = ArrayPool<byte>.Shared.Rent(t_size));

        bool success = emulatorProcess.ReadArray(realAddress, buffer);

        if (!success)
        {
            if (rented is not null)
                ArrayPool<byte>.Shared.Return(rented);
            return false;
        }

        if (Wiiemulator.Endianness == Endianness.Big)
            buffer.Reverse();

        unsafe
        {
            fixed (byte* ptr = buffer)
            {
                value = *(T*)ptr;
            }
        }

        if (rented is not null)
            ArrayPool<byte>.Shared.Return(rented);
        return true;
    }

    public override bool TryReadArray<T>(out T[] value, uint size, ulong address, params int[] offsets)
    {
        if (emulatorProcess is null || Wiiemulator is null || !ResolvePath(out IntPtr realAddress, address, offsets))
        {
            value = new T[size];
            return false;
        }

        int t_size = (int)size * Marshal.SizeOf<T>();

        byte[]? rented = null;
        Span<byte> buffer = t_size <= 1024
            ? stackalloc byte[t_size]
            : (rented = ArrayPool<byte>.Shared.Rent(t_size));

        bool success = emulatorProcess.ReadArray(realAddress, buffer);

        if (!success)
        {
            value = new T[size];
            if (rented is not null)
                ArrayPool<byte>.Shared.Return(rented);
            return false;
        }

        if (Wiiemulator.Endianness == Endianness.Big)
        {
            int s = Marshal.SizeOf<T>();
            for (int i = 0; i < size; i++)
                buffer[(s * i)..(s * (i + 1))].Reverse();
        }

        Span<T> newBuf = MemoryMarshal.Cast<byte, T>(buffer);
        value = newBuf[..(int)size].ToArray();
        if (rented is not null)
            ArrayPool<byte>.Shared.Return(rented);
        return true;
    }

}