// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System.Collections.Generic;
using SQLite.Net.Attributes;

namespace FX2.Game.Database
{
    /// <summary>
    /// A single group of difficulties that are placed in the same folder
    /// </summary>
    public class SetIndex
    {
        /// <summary>
        /// Locally unique Id of the set
        /// </summary>
        [Unique, PrimaryKey]
        public int Id { get; set; }

        /// <summary>
        /// Path to the map root folder
        /// </summary>
        public string RootPath { get; set; }
        
        public string Artist { get; set; }

        public string Title { get; set; }

        public string Creator { get; set; }

        public int MinimumBPM { get; set; }

        public int MaximumBPM { get; set; }

        /// <summary>
        /// Difficulties inside this beatmap set
        /// </summary>
        public readonly List<DifficultyIndex> Difficulties = new List<DifficultyIndex>();
    }
}