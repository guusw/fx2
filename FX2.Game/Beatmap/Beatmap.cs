// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System;
using System.Collections.Generic;
using System.Diagnostics;
using FX2.Game.Beatmap.Effects;

namespace FX2.Game.Beatmap
{
    /// <summary>
    /// Runtime beatmap format for FX2
    /// </summary>
    public partial class Beatmap
    {
        /// <summary>
        /// Additional data about the beatmap
        /// </summary>
        public BeatmapMetadata Metadata;

        /// <summary>
        /// The root of a beatmap consists of a timing point, containing measures, containing objects
        /// </summary>
        public readonly List<TimingPoint> TimingPoints = new List<TimingPoint>();

        /// <summary>
        /// Settings for effects that are used, indexed by <see cref="EffectType"/>
        /// </summary>
        public readonly List<EffectSettings> EffectSettings = new List<EffectSettings>();

        public Beatmap()
        {
            RegisterDefaultEffectSettings();
        }

        /// <summary>
        /// Creates annotations in every measure about the laser segments that cross it
        /// </summary>
        public void UpdateLaserIntervals()
        {
            List<ObjectReference> activeObjects = new List<ObjectReference>();
            foreach(var tp in TimingPoints)
            {
                foreach(var measure in tp.Measures)
                {
                    measure.CrossingObjects.Clear();
                    measure.CrossingObjects.AddRange(activeObjects);
                    foreach(var obj in measure.Objects)
                    {
                        var laser = obj as Laser;
                        if(laser != null)
                        {
                            var reference = new ObjectReference(laser, measure);
                            if(laser.Previous != null)
                                activeObjects.Remove(laser.Previous);
                            if(laser.Next == null)
                            {
                                activeObjects.Remove(reference);
                            }
                            else
                            {
                                activeObjects.Add(reference);
                            }
                            continue;
                        }

                        var hold = obj as Hold;
                        if(hold != null)
                        {
                            activeObjects.Add(new ObjectReference(hold, measure));
                            continue;
                        }

                        var holdEnd = obj as HoldEnd;
                        if(holdEnd != null)
                        {
                            activeObjects.Remove(holdEnd.StartingPoint);
                            continue;
                        }
                    }
                }
            }
        }

        public void RegisterDefaultEffectSettings()
        {
            RegisterEffect(EffectType.BitCrusher, new BitCrusherSettings());
            RegisterEffect(EffectType.Retrigger, new RetriggerSettings());
            RegisterEffect(EffectType.Echo, new EchoSettings());
            RegisterEffect(EffectType.Gate, new RetriggerSettings {LoopCount = 1});
            RegisterEffect(EffectType.Flanger, new FlangerSettings());
            RegisterEffect(EffectType.HighPassFilter, new BiQuadFilterSettings
            {
                FilterType = BiQuadFilterType.HighPass
            });
            RegisterEffect(EffectType.LowPassFilter, new BiQuadFilterSettings
            {
                FilterType = BiQuadFilterType.LowPass
            });
            RegisterEffect(EffectType.PeakingFilter, new BiQuadFilterSettings
            {
                FilterType = BiQuadFilterType.Peaking
            });
            RegisterEffect(EffectType.Phaser, new PhaserSettings());
            RegisterEffect(EffectType.SideChain, new SideChainSettings());
            RegisterEffect(EffectType.Wobble, new SideChainSettings());
        }

        public void RegisterEffect(EffectType effectType, EffectSettings settings)
        {
            while(EffectSettings.Count < (int)effectType)
                EffectSettings.Add(null);

            EffectSettings[(int)effectType] = settings;
        }

        public EffectSettings GetEffectSettings(EffectType effectType)
        {
            if((int)effectType >= EffectSettings.Count)
                return null;

            return EffectSettings[(int)effectType];
        }
    }
}