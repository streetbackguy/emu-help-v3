// The helper provides an automated way to look for supported emulators,
// depending on the ones that are explicitly enabled for each system.
//
// For convenience, the TryProcessHook method is run in every update loop,
// but is returned immediately if the last execution time is less than 1.5 seconds before.

using JHelper.Common.ProcessInterop;
using EmuHelp.Logging;
using System;
using System.Diagnostics;
using System.Linq;
#if LIVESPLIT
using EmuHelp.LiveSplit;
#endif

namespace EmuHelp.HelperBase;

public abstract partial class HelperBase
{
    /// <summary>
    /// The memory representation of the currently hooked emulator process.
    /// </summary>
    protected ProcessMemory? emulatorProcess;

    /// <summary>
    /// The emulator class associated with the currently hooked process.
    /// </summary>
    internal Emulator? emulatorClass;

    /// <summary>
    /// Timestamp of the last time a process task was executed.
    /// </summary>
    private DateTime lastProcessTask = DateTime.MinValue;
    private const double PROCESS_TASK_INTERVAL = 1.5;

    /// <summary>
    /// An array of process names that are explicitly allowed for hooking.
    /// Must be implemented in derived classes to provide specific process names.
    /// </summary>
    internal abstract string[] ProcessNames { get; }

    /// <summary>
    /// Abstract method to attach the emulator class. This must be implemented by derived classes.
    /// </summary>
    /// <returns>An instance of the emulator class or null if the operation fails.</returns>
    internal abstract Emulator? AttachEmuClass();

    /// <summary>
    /// Attempts to hook into the currently running emulator process.
    /// This method is called on each update loop, but returns early if 
    /// less than 1.5 seconds have passed since the last execution.
    /// </summary>
    private void TryProcessHook()
    {
        // Check if the last process hook was less than 1.5 seconds ago.
        // The wait interval can be changed through the WAIT_TIME variable
        if (lastProcessTask > DateTime.Now - TimeSpan.FromSeconds(PROCESS_TASK_INTERVAL))
            return;

        // Update the last execution time to the current time
        lastProcessTask = DateTime.Now;

        try
        {
#if LIVESPLIT
            // Get the current game process from the Autosplitter
            Process process = Autosplitter.Game;
            string processName = process.ProcessName;

            // Check if the current process is valid and matches the allowed process names
            // Exclude the LiveSplit process from being hooked
            if (ProcessNames.Contains(processName) && !processName.Equals("livesplit", StringComparison.OrdinalIgnoreCase))
            {
                emulatorProcess = ProcessMemory.HookProcess(process.Id);
                return;
            }
            // If the current process is not valid or allowed, search for any allowed process
            else
            {
#endif
                ProcessMemory? _process = ProcessNames
                    .Select(ProcessMemory.HookProcess)
                    .FirstOrDefault(p => p is not null);

                if (_process is not null)
                    emulatorProcess = _process;

                return;
#if LIVESPLIT
            }
#endif
        }
        catch
        {
            return;
        }
    }

    /// <summary>
    /// Handles the exit of the emulator process, cleaning up references and logging.
    /// </summary>
    private void HandleProcessExit()
    {
        emulatorClass = null;       // Clear the emulator reference
        emulatorProcess?.Dispose();    // Dispose of the process object
        emulatorProcess = null;        // Set the process to null
        Log.Info("  => Detached from emulator");   // Log the detachment
    }
}