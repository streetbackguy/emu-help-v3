using System;
using System.Linq;
using EmuHelp.Logging;
using JHelper.Common.ProcessInterop;
using EmuHelp.HelperBase;

namespace EmuHelp.Systems.SNES.Emulators
{
    internal class Retroarch : SNESEmulator
    {
        private IntPtr core_base_address;

        public IntPtr RamBase { get; private set; }

        // List of supported Retroarch SNES cores
        private readonly string[] supportedCores =
        {
            "snes9x_libretro.dll",
            "bsnes_libretro.dll",
            "bsnes_balanced_libretro.dll",
            "snes9x_next_libretro.dll"
        };

        internal Retroarch()
        {
            Log.Info("  => Attached to emulator: Retroarch (SNES)");
        }

        // Abstract method implementation: find WRAM in the core
        public override bool FindRAM(ProcessMemory process)
        {
            ProcessModule core = process.Modules.FirstOrDefault(m => supportedCores.Contains(m.ModuleName));
            if (core == default)
                return false;

            core_base_address = core.BaseAddress;

            if (!FindWRAM(process, out IntPtr wram))
                return false;

            RamBase = wram;
            Log.Info($"  => WRAM found at 0x{RamBase.ToString("X")}");
            return true;
        }

        // Abstract method implementation: ensure emulator process is alive
        public override bool KeepAlive(ProcessMemory process)
        {
            return process.Read(core_base_address, out byte _);
        }

        // Internal helper method to scan for WRAM
        private bool FindWRAM(ProcessMemory process, out IntPtr wram)
        {
            wram = IntPtr.Zero;

            // relaxed memory page scan
            MemoryPage page = process.MemoryPages
                .Where(p => p.State == MemoryState.MEM_COMMIT &&
                            p.Type == MemoryType.MEM_PRIVATE &&
                            p.Protect == MemoryProtection.PAGE_READWRITE &&
                            p.RegionSize >= 0x20000)
                .OrderByDescending(p => p.BaseAddress.ToInt64()) // pick highest page
                .FirstOrDefault(); // no extra parenthesis here

            if (page != null)
            {
                wram = page.BaseAddress;
                core_base_address = page.BaseAddress;
                return true;
            }

            Log.Info("FindWRAM: Memory page scan failed, AOB scan fallback not implemented for this core.");
            return false;
        }
    }
}
