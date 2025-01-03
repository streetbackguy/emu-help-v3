using Helper.Common.ProcessInterop;

namespace Helper.HelperBase;

/// <summary>
/// Abstract base class for emulators that manage 
/// interactions with the memory of the emulated system. 
/// This class provides properties and methods for finding 
/// RAM addresses and ensuring the emulator remains active.
/// </summary>
public abstract class Emulator
{
    /// <summary>
    /// Gets or sets a value indicating whether RAM has been found for 
    /// the associated emulator process.
    /// </summary>
    public bool FoundRam { get; set; } = false;

    /// <summary>
    /// Attempts to find the RAM address associated with the specified process.
    /// This method must be implemented in derived classes to provide 
    /// specific logic for different emulator types.
    /// </summary>
    /// <param name="process">The process of the emulator to search for RAM.</param>
    /// <returns>True if RAM was successfully found; otherwise, false.</returns>
    public abstract bool FindRAM(ProcessMemory process);

    /// <summary>
    /// Ensures that the emulator remains active for the specified process.
    /// This method must be implemented in derived classes to provide 
    /// specific logic for different emulator types.
    /// </summary>
    /// <param name="process">The process of the emulator to keep alive.</param>
    /// <returns>True if the emulator is kept alive; otherwise, false.</returns>
    public abstract bool KeepAlive(ProcessMemory process);
}