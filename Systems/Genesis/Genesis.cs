using EmuHelp.HelperBase;
using EmuHelp.Systems.Genesis;
using EmuHelp.Systems.Genesis.Emulators;
using System;
using System.Runtime.InteropServices;
using EmuHelp.Logging;
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
#if LIVESPLIT
        : this(true) { }

    public Genesis(bool generateCode)
        : base(generateCode)
#else
        : base()
#endif
    {
        Log.Info("  => SEGA Genesis Helper started");
    }

    internal override string[] ProcessNames { get; } =
    [
        "retroarch.exe",
        "SEGAGameRoom.exe",
        "SEGAGenesisClassics.exe",
        "Fusion.exe",
        "gens.exe",
        "blastem.exe",
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
            "retroarch.exe" => new Retroarch(),
            "SEGAGameRoom.exe" or "SEGAGenesisClassics.exe" => new SegaClassics(),
            "Fusion.exe" => new Fusion(),
            "gens.exe" => new Gens(),
            "blastem.exe" => new BlastEm(),
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

        int size;
        unsafe
        {
            size = sizeof(T);
        }

        using (ArrayRental<byte> buffer = size <= 1022 ? new ArrayRental<byte>(stackalloc byte[size + 2]) : new ArrayRental<byte>(size +2))
        {
            int shift = (size + misalignment) % 2;

            Span<byte> newBuf = buffer.Span[(misalignment ^ 1)..][..(size + misalignment + shift)];

            bool success = emulatorProcess.ReadArray(realAddress, newBuf);

            if (!success)
                return false;

            if (Genesisemulator.Endianness == Endianness.Little)
            {
                for (int i = 0; i < newBuf.Length; i += 2)
                    newBuf[i..(i + 2)].Reverse();
            }

            buffer.Span[1..(1 + size)].Reverse();

            unsafe
            {
                fixed (byte* ptr = buffer.Span)
                {
                    value = *(T*)(ptr + 1);
                }
            }

        }

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

        int sizeofT;
        unsafe
        {
            sizeofT = sizeof(T);
        }

        int size = sizeofT * (int)arraySize;

        using (ArrayRental<byte> buffer = size <= 1022 ? new(stackalloc byte[size +2]) : new(size + 2))
        {

            int shift = (size + misalignment) % 2;
            Span<byte> newBuf = buffer.Span[(misalignment ^ 1)..][..(size + misalignment + shift)];
            bool success = emulatorProcess.ReadArray(realAddress, newBuf);

            if (!success)
            {
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
                buffer.Span[1..][(i * sizeofT)..][..sizeofT].Reverse();
            }

            value = MemoryMarshal.Cast<byte, T>(buffer.Span[1..][..size]).ToArray();
        }

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

        using (ArrayRental<byte> buffer = size <= 1022 ? new(stackalloc byte[size + 2]) : new(size + 2))
        {
            int shift = (size + misalignment) % 2;
            Span<byte> newBuf = buffer.Span[(misalignment ^ 1)..][..(size + misalignment + shift)];
            bool success = emulatorProcess.ReadArray(realAddress, newBuf);

            if (!success)
                return false;

            if (Genesisemulator.Endianness == Endianness.Little)
            {
                for (int i = 0; i < newBuf.Length; i += 2)
                    newBuf[i..(i + 2)].Reverse();
            }

            if (stringSize >= 2 && buffer.Span[1..] is [> 0, 0, > 0, 0, ..])
            {
                Span<char> charBuffer = MemoryMarshal.Cast<byte, char>(buffer.Span[1..]);
                int length = charBuffer[..(int)stringSize].IndexOf('\0');
                value = length == -1 ? charBuffer[..(int)stringSize].ToString() : buffer.Span[1..][..(int)stringSize][..length].ToString();
            }
            else
            {
                int length = buffer.Span[1..][..(int)stringSize].IndexOf((byte)0);

                unsafe
                {
                    fixed (byte* pBuffer = buffer.Span[1..])
                    {
                        value = new string((sbyte*)pBuffer, 0, length == -1 ? (int)stringSize : length);
                    }
                }
            }
        }

        return true;
    }
}