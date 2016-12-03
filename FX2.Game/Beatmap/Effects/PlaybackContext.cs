// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using osu.Framework.Audio.Track;

namespace FX2.Game.Beatmap.Effects
{
    /// <summary>
    /// Information about a beatmap being played
    /// </summary>
    public class PlaybackContext
    {
        public BeatmapPlayback Playback;
        public AudioTrack Track;
    }
}