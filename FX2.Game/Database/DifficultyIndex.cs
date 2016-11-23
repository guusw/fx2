// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System;
using System.ComponentModel.DataAnnotations.Schema;
using FX2.Game.Beatmap;
using SQLite.Net;
using SQLite.Net.Attributes;

namespace FX2.Game.Database
{
    /// <summary>
    /// A single difficulty entry in the database
    /// </summary>
    public class DifficultyIndex
    {
        /// <summary>
        /// Locally unique Id of this difficulty
        /// </summary>
        [Unique, PrimaryKey]
        public int Id { get; set; }
        
        /// <summary>
        /// Set that contains this difficulty
        /// </summary>
        [ForeignKey("SetIndex")]
        public int SetId { get; set; }

        /// <summary>
        /// Full path to the difficulty file
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Last write time of this difficulty
        /// </summary>
        public DateTime LastWriteTime { get; set; }
        
        public BeatmapMetadata MetaData { get; set; }
    }
}