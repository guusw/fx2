// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System;
using System.Collections.Generic;

namespace FX2.Game.Beatmap
{
    [Serializable]
    public class BeatmapMetadata
    {
        /// <summary>
        /// Song title
        /// </summary>
        public string Title = "";

        /// <summary>
        /// Song artist
        /// </summary>
        public string Artist = "";

        /// <summary>
        /// Creator of the map
        /// </summary>
        public string Creator = "";

        /// <summary>
        /// Illustrator of the jacked image
        /// </summary>
        public string Illustrator = "";

        /// <summary>
        /// Set of additional tags for this beatmap
        /// </summary>
        public HashSet<string> Tags = new HashSet<string>();

        /// <summary>
        /// Path to the beatmap cover illustration (jacket), relative to the beatmap folder
        /// </summary>
        public string JacketPath = "";
        
        /// <summary>
        /// Path to the beatmap's main audio file, relative to the beatmap folder
        /// </summary>
        public string AudioPath = "";

        /// <summary>
        /// Path to the beatmap's effected audio file, if any
        /// </summary>
        public string EffectedAudioPath = "";
        
        /// <summary>
        /// Preview offset
        /// </summary>
        public double PreviewOffset = 0.0;

        /// <summary>
        /// Length of the previewed area
        /// </summary>
        public double PreviewDuration = 0.0;
    }
}