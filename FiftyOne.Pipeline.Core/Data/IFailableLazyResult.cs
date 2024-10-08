using System;
using System.Collections.Generic;
using System.Text;

namespace FiftyOne.Pipeline.Core.Data
{
    /// <summary>
    /// Allows result instances to indicate
    /// if the result itself is a temporary fallback
    /// and should not be saved/cached yet.
    /// </summary>
    public interface IFailableLazyResult
    {
        /// <summary>
        /// Whether the result may be safely saved/cached and reused.
        /// </summary>
        bool MayBeSaved { get; }
    }
}
