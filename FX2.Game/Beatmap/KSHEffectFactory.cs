// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System.Collections.Generic;
using FX2.Game.Beatmap.Effects;

namespace FX2.Game.Beatmap
{
    /// <summary>
    /// A factory for custom effect definition from ksh maps
    /// </summary>
    public abstract class KshEffectFactory
    {
        /// <summary>
        /// Contains effect base types for which this factory can generate settings for.
        /// </summary>
        public abstract IEnumerable<EffectType> SupportedEffectTypes { get; }

        /// <summary>
        /// Generates effect settings from a KSH effect definition
        /// </summary>
        /// <param name="effectDefinition"></param>
        /// <returns></returns>
        public abstract EffectSettings GenerateEffectSettings(BeatmapKsh.EffectDefinition effectDefinition);
    }
}