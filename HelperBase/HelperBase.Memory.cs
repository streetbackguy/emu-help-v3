// The helper provides a way to directly read a value from the emulated system's memory.

using System;

namespace Helper.HelperBase;

public abstract partial class HelperBase
{
    /// <summary>
    /// Gets the real memory address corresponding to the specified mapped address.
    /// </summary>
    /// <param name="address">The mapped address to resolve.</param>
    /// <returns>The resolved memory address as an <see cref="IntPtr"/>.</returns>
    public IntPtr GetRealAddress(ulong address)
    {
        return TryGetRealAddress(address, out IntPtr realAddress)
            ? realAddress
            : default;
    }

    /// <summary>
    /// Attempts to resolve the real memory address from a mapped address.
    /// </summary>
    /// <param name="address">The mapped address to resolve.</param>
    /// <param name="realAddress">The resolved memory address as an output parameter.</param>
    /// <returns>True if the address was successfully resolved; otherwise, false.</returns>
    public abstract bool TryGetRealAddress(ulong address, out IntPtr realAddress);

    /// <summary>
    /// Reads a value of type <typeparamref name="T"/> from the specified 
    /// address in memory, applying the provided offsets.
    /// </summary>
    /// <typeparam name="T">The type of the value to read. Must be unmanaged.</typeparam>
    /// <param name="address">The base address in memory to read from.</param>
    /// <param name="offsets">An array of offsets to apply to the base address.</param>
    /// <returns>The read value of type <typeparamref name="T"/>; or the default value if reading fails.</returns>
    public T Read<T>(ulong address, params int[] offsets) where T : unmanaged
    {
        return TryRead(out T value, address, offsets)
            ? value
            : default;
    }

    /// <summary>
    /// Attempts to read a value of type <typeparamref name="T"/> from 
    /// the specified address, applying the provided offsets.
    /// </summary>
    /// <typeparam name="T">The type of the value to read. Must be unmanaged.</typeparam>
    /// <param name="value">The read value, set as an output parameter.</param>
    /// <param name="address">The base address in memory to read from.</param>
    /// <param name="offsets">An array of offsets to apply to the base address.</param>
    /// <returns>True if the value was successfully read; otherwise, false.</returns>
    public virtual bool TryRead<T>(out T value, ulong address, params int[] offsets) where T : unmanaged
    {
        if (emulatorProcess is null
            || !ResolvePath(out IntPtr realAddress, address, offsets))
        {
            value = default;
            return false;
        }

        return emulatorProcess.Read<T>(realAddress, out value);
    }

    /// <summary>
    /// Reads an array of values of type <typeparamref name="T"/> 
    /// from the specified address in memory, applying the provided offsets.
    /// </summary>
    /// <typeparam name="T">The type of the values to read. Must be unmanaged.</typeparam>
    /// <param name="size">The number of elements in the array to read.</param>
    /// <param name="address">The base address in memory to read from.</param>
    /// <param name="offsets">An array of offsets to apply to the base address.</param>
    /// <returns>An array of read values of type <typeparamref name="T"/>; or an empty array if reading fails.</returns>
    public T[] ReadArray<T>(uint size, ulong address, params int[] offsets) where T : unmanaged
    {
        return TryReadArray(out T[] value, size, address, offsets)
            ? value
            : new T[(int)size];
    }

    /// <summary>
    /// Attempts to read an array of values of type <typeparamref name="T"/> 
    /// from the specified address in memory, applying the provided offsets.
    /// </summary>
    /// <typeparam name="T">The type of the values to read. Must be unmanaged.</typeparam>
    /// <param name="value">The read array, set as an output parameter.</param>
    /// <param name="size">The number of elements in the array to read.</param>
    /// <param name="address">The base address in memory to read from.</param>
    /// <param name="offsets">An array of offsets to apply to the base address.</param>
    /// <returns>True if the array was successfully read; otherwise, false.</returns>
    public virtual bool TryReadArray<T>(out T[] value, uint size, ulong address, params int[] offsets) where T : unmanaged
    {
        value = new T[size];

        if (emulatorProcess is null
            || !ResolvePath(out IntPtr realAddress, address, offsets)
            || emulatorProcess.ReadArray<T>(realAddress, value))
            return false;

        return true;
    }

    /// <summary>
    /// Reads a null-terminated string from memory at the specified address, applying the provided offsets.
    /// </summary>
    /// <param name="stringSize">The maximum length of the string to read from memory.</param>
    /// <param name="address">The base address in memory to read from.</param>
    /// <param name="offsets">An array of offsets to apply to the base address.</param>
    /// <returns>The read string; or an empty string if reading fails.</returns>
    public string ReadString(uint stringSize, ulong address, params int[] offsets)
    {
        return TryReadString(out string value, stringSize, address, offsets)
            ? value
            : string.Empty;
    }

    /// <summary>
    /// Attempts to read a null-terminated string from memory at the specified address, applying the provided offsets.
    /// </summary>
    /// <param name="value">The read string, set as an output parameter.</param>
    /// <param name="stringSize">The maximum length of the string to read from memory.</param>
    /// <param name="address">The base address in memory to read from.</param>
    /// <param name="offsets">An array of offsets to apply to the base address.</param>
    /// <returns>True if the string was successfully read; otherwise, false.</returns>
    public virtual bool TryReadString(out string value, uint stringSize, ulong address, params int[] offsets)
    {
        // Check if the emulator process is valid and retrieve the real address for the base address
        if (emulatorProcess is null || !ResolvePath(out IntPtr realAddress, address, offsets))
        {
            value = string.Empty;
            return false;
        }

        return emulatorProcess.ReadString(realAddress, (int)stringSize, out value);
    }

    /// <summary>
    /// Resolves the final address in memory by applying the provided offsets 
    /// to the base address. This method first retrieves the real address for 
    /// the base address, then applies each offset in succession.
    /// </summary>
    /// <param name="finalAddress">The final resolved address as an output parameter.</param>
    /// <param name="baseAddress">The base address in memory to resolve.</param>
    /// <param name="offsets">An array of offsets to apply to the base address.</param>
    /// <returns>True if the final address was successfully resolved; otherwise, false.</returns>
    protected virtual bool ResolvePath(out IntPtr finalAddress, ulong baseAddress, params int[] offsets)
    {
        // Check if the emulator process is valid and retrieve the real address for the base address
        if (emulatorProcess is null || !TryGetRealAddress(baseAddress, out finalAddress))
        {
            finalAddress = default;
            return false;
        }

        foreach (int offset in offsets)
        {
            if (!emulatorProcess.Read(finalAddress, out uint tempAddress)
                || !TryGetRealAddress((ulong)(tempAddress + offset), out finalAddress))
                return false;
        }

        return true;
    }
}
