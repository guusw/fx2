// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using osu.Framework.Audio.Track;

namespace FX2.Game.Beatmap.Effects
{
    /// <summary>
    /// Base class for something that describes how an audio effect behaves, what dsp's it uses, etc.
    /// </summary>
    public abstract class EffectSettings
    {
        /// <summary>
        /// Creates a new effect state on a given audio track
        /// </summary>
        public abstract EffectState CreateEffectState(PlaybackContext playbackContext);
    }
}