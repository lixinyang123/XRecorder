﻿using System;

namespace Xabe.FFmpeg
{
    /// <summary>
    ///     Information about conversion
    /// </summary>
    public interface IConversionResult
    {
        /// <summary>
        ///     Date and time of starting conversion
        /// </summary>
        DateTime StartTime { get; }

        /// <summary>
        ///     Date and time of starting conversion
        /// </summary>
        DateTime EndTime { get; }

        /// <summary>
        ///     Conversion duration
        /// </summary>
        TimeSpan Duration { get; }

        /// <summary>
        ///     Arguments passed to ffmpeg
        /// </summary>
        string Arguments { get; }
    }
}
