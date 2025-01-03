// Inspired by asl-help, by just-ero.
// asl-help is licensed under GPL-3.0.
// See: https://github.com/just-ero/asl-help

using LiveSplit.Model;
using LiveSplit.UI;
using LiveSplit.View;
using System.Windows.Forms;

namespace Helper.LiveSplit;

internal static class Timer
{
    public static LiveSplitState State { get; } = ((TimerForm)Application.OpenForms["TimerForm"]).CurrentState;

    public static IRun Run { get => State.Run; set => State.Run = value; }

    public static ILayout Layout { get => State.Layout; set => State.Layout = value; }

    public static Form Form { get => State.Form; set => State.Form = value; }

    public static TimingMethod CurrentTimingMethod { get => State.CurrentTimingMethod; set => State.CurrentTimingMethod = value; }
}