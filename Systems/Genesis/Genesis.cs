using Helper.HelperBase;
using Helper.Genesis;
using Helper.Genesis.Emulators;
using System;
using System.Buffers;
using System.Runtime.InteropServices;
using Helper.Logging;
using JHelper.Common.MemoryUtils;

public class SegaGenesis : Genesis { }

public class Genesis : HelperBase
{
    private const uint MINSIZE = 0;
    private const uint MAXSIZE = 0x10000;
    private const uint MINSIZE_ALT = 0xFF0000;
    private const uint MAXSIZE_ALT = 0x1000000;

    private GenesisEmulator? Genesisemulator
    {
        get => (GenesisEmulator?)emulatorClass;
        set => emulatorClass = value;
    }

    public Genesis()
        : this(true) { }

    public Genesis(bool generateCode)
        : base(generateCode)
    {
        Log.Info("  => SEGA Genesis Helper started");
    }

    internal override string[] ProcessNames { get; } =
    [
        "retroarch",
        "SEGAGameRoom",
        "SEGAGenesisClassics",
        "Fusion",
        "gens",
        "blastem",
    ];

    public override bool TryGetRealAddress(ulong address, out IntPtr realAddress)
    {
        realAddress = default;

        if (Genesisemulator is null)
            return false;

        IntPtr baseRam = Genesisemulator.RamBase;

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
            "retroarch" => new Retroarch(),
            "SEGAGameRoom" or "SEGAGenesisClassics" => new SegaClassics(),
            "Fusion" => new Fusion(),
            "gens" => new Gens(),
            "blastem" => new BlastEm(),
            _ => null,
        };
    }

    public override bool TryRead<T>(out T value, ulong address, params int[] offsets)
    {
        value = default;

        if (emulatorProcess is null || Genesisemulator is null)
            return false;

        byte misalignment = (byte)(address & 1);
        ulong alignedAddress = address - misalignment;

        if (!ResolvePath(out IntPtr realAddress, alignedAddress))
            return false;

        int size = Marshal.SizeOf<T>();

        byte[]? rented = null;
        Span<byte> buffer = size <= 1022
            ? stackalloc byte[size + 2]
            : (rented = ArrayPool<byte>.Shared.Rent(size + 2));

        int shift = (size + misalignment) % 2;

        Span<byte> newBuf = buffer[(misalignment ^ 1)..][..(size + misalignment + shift)];

        bool success = emulatorProcess.ReadArray(realAddress, newBuf);

        if (!success)
        {
            if (rented is not null)
                ArrayPool<byte>.Shared.Return(rented);

            return false;
        }

        if (Genesisemulator.Endianness == Endianness.Little)
        {
            for (int i = 0; i < newBuf.Length; i += 2)
                newBuf[i..(i + 2)].Reverse();
        }

        buffer[1..(1 + size)].Reverse();

        unsafe
        {
            fixed (byte* ptr = buffer)
            {
                value = *(T*)(ptr + 1);
            }
        }

        if (rented is not null)
            ArrayPool<byte>.Shared.Return(rented);

        return true;
    }

    public override bool TryReadArray<T>(out T[] value, uint arraySize, ulong address, params int[] offsets)
    {
        if (emulatorProcess is null || Genesisemulator is null)
        {
            value = new T[arraySize];
            return false;
        }

        byte misalignment = (byte)(address & 1);
        ulong alignedAddress = address - misalignment;

        if (!ResolvePath(out IntPtr realAddress, alignedAddress))
        {
            value = new T[arraySize];
            return false;
        }

        int sizeofT = Marshal.SizeOf<T>();
        int size = sizeofT * (int)arraySize;

        byte[]? rented = null;
        Span<byte> buffer = size <= 1022
            ? stackalloc byte[size + 2]
            : (rented = ArrayPool<byte>.Shared.Rent(size + 2));

        int shift = (size + misalignment) % 2;
        Span<byte> newBuf = buffer[(misalignment ^ 1)..][..(size + misalignment + shift)];
        bool success = emulatorProcess.ReadArray(realAddress, newBuf);

        if (!success)
        {
            if (rented is not null)
                ArrayPool<byte>.Shared.Return(rented);
            value = new T[arraySize];
            return false;
        }

        if (Genesisemulator.Endianness == Endianness.Little)
        {
            for (int i = 0; i < newBuf.Length; i += 2)
                newBuf[i..(i + 2)].Reverse();
        }

        for (int i = 0; i < arraySize; i++)
        {
            buffer[1..][(i * sizeofT)..][..sizeofT].Reverse();
        }

        value = MemoryMarshal.Cast<byte, T>(buffer[1..][..size]).ToArray();

        if (rented is not null)
            ArrayPool<byte>.Shared.Return(rented);
        return true;
    }

    public override bool TryReadString(out string value, uint stringSize, ulong address, params int[] offsets)
    {
        value = string.Empty;

        if (emulatorProcess is null || Genesisemulator is null)
            return false;

        byte misalignment = (byte)(address & 1);
        ulong alignedAddress = address - misalignment;

        if (!ResolvePath(out IntPtr realAddress, alignedAddress))
            return false;

        int size = (int)stringSize * 2;

        byte[]? rented = null;
        Span<byte> buffer = size <= 1022
            ? stackalloc byte[size + 2]
            : (rented = ArrayPool<byte>.Shared.Rent(size + 2));

        int shift = (size + misalignment) % 2;
        Span<byte> newBuf = buffer[(misalignment ^ 1)..][..(size + misalignment + shift)];
        bool success = emulatorProcess.ReadArray(realAddress, newBuf);

        if (!success)
        {
            if (rented is not null)
                ArrayPool<byte>.Shared.Return(rented);
            return false;
        }

        if (Genesisemulator.Endianness == Endianness.Little)
        {
            for (int i = 0; i < newBuf.Length; i += 2)
                newBuf[i..(i + 2)].Reverse();
        }

        if (stringSize >= 2 && buffer[1..] is [> 0, 0, > 0, 0, ..])
        {
            Span<char> charBuffer = MemoryMarshal.Cast<byte, char>(buffer[1..]);
            int length = charBuffer[..(int)stringSize].IndexOf('\0');
            value = length == -1 ? charBuffer[..(int)stringSize].ToString() : buffer[1..][..(int)stringSize][..length].ToString();
        }
        else
        {
            int length = buffer[1..][..(int)stringSize].IndexOf((byte)0);

            unsafe
            {
                fixed (byte* pBuffer = buffer[1..])
                {
                    value = new string((sbyte*)pBuffer, 0, length == -1 ? (int)stringSize : length);
                }
            }
        }

        if (rented is not null)
            ArrayPool<byte>.Shared.Return(rented);
        return true;
    }
}