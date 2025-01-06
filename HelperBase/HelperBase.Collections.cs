using JHelper.Common.MemoryUtils;
using System.Collections.Generic;

namespace Helper.HelperBase.Collections;

/// <summary>
/// A <see cref="Dictionary{TKey, TValue}"/> of <see cref="Watcher{T}"/>
/// that can be used as a generic collection for any type of <see cref="Watcher{T}"/>
/// defined in an auto splitter
/// </summary>
public class WatcherDictionary : Dictionary<string, Watcher>
{
    /// <summary>
    /// Updates the <see cref="Watcher{T}.Current"/> value for every <see cref="Watcher{T}"/>
    /// defined inside the Dictionary.
    /// </summary>
    public void UpdateAll()
    {
        foreach (Watcher watcher in Values)
            watcher.Update();
    }

    /// <summary>
    /// Resets every <see cref="Watcher{T}"/>
    /// defined inside the Dictionary.
    /// </summary>
    public void ResetAll()
    {
        foreach (Watcher watcher in Values)
            watcher.Reset();
    }
}
