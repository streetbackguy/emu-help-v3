using EmuHelp.HelperBase;
using EmuHelp.Systems.GCN;
using EmuHelp.Systems.GCN.Emulators;
using System;
using System.Buffers;
using System.Runtime.InteropServices;
using EmuHelp.Logging;
using JHelper.Common.MemoryUtils;

public class GameCube : GCN { }

public class Gamecube : GCN { }

public class GCN : HelperBase
{
    private const uint MINSIZE = 0x80000000;
    private const uint MAXSIZE = 0x81800000;

    private GCNEmulator? Gcnemulator
    {
        get => (GCNEmulator?)emulatorClass;
        set => emulatorClass = value;
    }

    public GCN()
#if LIVESPLIT
        : this(true) { }

    public GCN(bool generateCode)
        : base(generateCode)
#else
        : base()
#endif
    {
        Log.Info("  => GCN Helper started");
    }

    internal override string[] ProcessNames { get; } =
    [
        "Dolphin.exe",
        "retroarch.exe",
    ];

    public override bool TryGetRealAddress(ulong address, out IntPtr realAddress)
    {
        realAddress = default;

        if (Gcnemulator is null)
            return false;

        IntPtr baseRam = Gcnemulator.MEM1;

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
            "Dolphin.exe" => new Dolphin(),
            "retroarch.exe" => new Retroarch(),
            _ => null,
        };
    }

    public override bool TryRead<T>(out T value, ulong address, params int[] offsets)
    {
        value = default;

        if (emulatorProcess is null || Gcnemulator is null || !ResolvePath(out IntPtr realAddress, address, offsets))
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

        if (Gcnemulator.Endianness == Endianness.Big)
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
        if (emulatorProcess is null || Gcnemulator is null || !ResolvePath(out IntPtr realAddress, address, offsets))
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

        if (Gcnemulator.Endianness == Endianness.Big)
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