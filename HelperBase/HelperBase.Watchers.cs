// Defining the Make methods for creating memory watchers
// to easily peek into the emulated games' memory

using JHelper.Common.Collections;
using JHelper.Common.MemoryUtils;

namespace EmuHelp.HelperBase;

public abstract partial class HelperBase
{
    /// <summary>
    /// The tick counter used to lazily evaluate data between update cycles.
    /// </summary>
    internal MemStateTracker _tickCounter = new();

    /// <summary>
    /// Creates a <see cref="LazyWatcher{T}"/> for a single memory value 
    /// of the specified type <typeparamref name="T"/>. The watcher can 
    /// be used to monitor changes to the value at the specified base address 
    /// and offsets.
    /// </summary>
    /// <typeparam name="T">The type of the memory value to watch. Must be unmanaged.</typeparam>
    /// <param name="baseAddress">The base address in memory where the value resides.</param>
    /// <param name="offsets">An array of offsets to reach the target memory address.</param>
    /// <returns>A <see cref="LazyWatcher{T}"/> that provides access to the memory value.</returns>
    public virtual LazyWatcher<T> Make<T>(ulong baseAddress, params int[] offsets) where T : unmanaged
    {
        return new LazyWatcher<T>(_tickCounter, default, (_, _) => this.Read<T>(baseAddress, offsets));
    }

    /// <summary>
    /// Creates a <see cref="LazyWatcher{T[]}"/> for an array of memory values 
    /// of the specified type <typeparamref name="T"/>. The watcher allows 
    /// monitoring of an array of values at the specified base address and offsets.
    /// </summary>
    /// <typeparam name="T">The type of the memory values to watch. Must be unmanaged.</typeparam>
    /// <param name="size">The size of the array to be monitored.</param>
    /// <param name="baseAddress">The base address in memory where the array starts.</param>
    /// <param name="offsets">An array of offsets to reach the target memory address.</param>
    /// <returns>A <see cref="LazyWatcher{T[]}"/> that provides access to the array of memory values.</returns>
    public virtual LazyWatcher<T[]> MakeArray<T>(uint size, ulong baseAddress, params int[] offsets) where T : unmanaged
    {
        return new LazyWatcher<T[]>(_tickCounter, new T[size], (_, _) => this.ReadArray<T>(size, baseAddress, offsets));
    }

    /// <summary>
    /// Creates a <see cref="LazyWatcher{string}"/> for a string value stored 
    /// in memory. The watcher monitors changes to the string at the specified 
    /// base address and offsets, with a defined maximum length.
    /// </summary>
    /// <param name="maxLength">The maximum length of the string to read from memory.</param>
    /// <param name="baseAddress">The base address in memory where the string starts.</param>
    /// <param name="offsets">An array of offsets to reach the target memory address.</param>
    /// <returns>A <see cref="LazyWatcher{string}"/> that provides access to the memory string.</returns>
    public virtual LazyWatcher<string> MakeString(uint maxLength, ulong baseAddress, params int[] offsets)
    {
        return new LazyWatcher<string>(_tickCounter, string.Empty, (_, _) => this.ReadString(maxLength, baseAddress, offsets));
    }
}
