using JHelper.Common.ProcessInterop;
using Microsoft.Diagnostics.Runtime;
using System;
using System.Linq;

namespace EmuHelp.Multisystems
{
    internal static class EmuHawk
    {
        internal static bool MainForm(ProcessMemory process, out IntPtr instance, out int emulatorOffset)
        {
            instance = IntPtr.Zero;
            emulatorOffset = 0;

            // Attempts to find the MainForm instance and emulator offset
            using (DataTarget target = DataTarget.AttachToProcess(process.Id, false))
            {
                ClrHeap heap = target.ClrVersions[0].CreateRuntime().Heap;
                if (!heap.CanWalkHeap)
                    return false;

                ClrObject obj;
                using (var enumerator = heap
                    .EnumerateObjects()
                    .Where(obj => obj.Type?.Name == "BizHawk.Client.EmuHawk.MainForm")
                    .GetEnumerator())
                {
                    if (enumerator.MoveNext())
                        obj = enumerator.Current;
                    else
                        return false;
                }

                ClrType? type = obj.Type;
                if (type is null)
                    return false;

                instance = (IntPtr)obj.Address;
                emulatorOffset = type.Fields.First(f => f.Name == "_emulator").Offset;
            }

            return true;
        }
    }
}
