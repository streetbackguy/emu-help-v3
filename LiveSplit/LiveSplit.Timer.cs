// Inspired by asl-help, by just-ero.
// asl-help is licensed under GPL-3.0.
// See: https://github.com/just-ero/asl-help

using Helper.Common.Reflection;
using LiveSplit.Model;
using LiveSplit.UI;
using LiveSplit.View;
using System;
using System.Linq;
using System.Reflection;

namespace Helper.LiveSplit;

internal static class Timer
{
    public static LiveSplitState State { get; }

    public static IRun Run { get => State.Run; set => State.Run = value; }

    public static ILayout Layout { get => State.Layout; set => State.Layout = value; }

    public static TimingMethod CurrentTimingMethod { get => State.CurrentTimingMethod; set => State.CurrentTimingMethod = value; }

    static Timer()
    {
        Assembly windowsFormAssembly = AppDomain.CurrentDomain
            .GetAssemblies()
            .First(a => a.FullName.StartsWith("System.Windows.Forms"));

        object openForms = windowsFormAssembly
            .GetType("System.Windows.Forms.Application")
            .GetProperty("OpenForms", BindingFlags.Public | BindingFlags.Static)
            .GetValue(null);
        
        int count = openForms.GetPropertyValue<int>("Count");
        PropertyInfo itemProperty = openForms.GetType().GetProperty("Item");

        for (int i = 0; i < count; i++)
        {
            object form = itemProperty.GetValue(openForms, [i]);
            string name = form.GetPropertyValue<string>("Name");
            if (name == "TimerForm")
            {
                State = ((TimerForm)form).CurrentState;
                break;
            }
        }

        if (State is null)
            throw new ArgumentNullException();
    }
}