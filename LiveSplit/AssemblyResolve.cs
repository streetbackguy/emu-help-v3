using System;
using System.Reflection;

namespace Helper.LiveSplit;

public static class LiveSplitAssembly
{
    public static void AssemblyResolveSubscribe()
    {
        AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
    }

    public static void AssemblyResolveUnsubscribe()
    {
        AppDomain.CurrentDomain.AssemblyResolve -= AssemblyResolve;
    }

    /// <summary>
    /// Resolves the assembly specified in the <see cref="ResolveEventArgs"/>.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event arguments containing the assembly name.</param>
    /// <returns>The loaded assembly, or null if loading fails.</returns>
    private static Assembly AssemblyResolve(object? sender, ResolveEventArgs e)
    {
        string name = e.Name; // Get the requested assembly name
        int i = name.IndexOf(',');

        // If there is no comma in the name, return null
        if (i == -1)
            return null!;

        return Assembly.LoadFrom($"Components/{name[..i]}.dll");
    }
}
