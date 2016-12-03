// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FX2.Game.Beatmap;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;

namespace FX2.Game.Audio
{
    public class EffectState : IDisposable
    {
        public ObjectReference AttachedObject;

        public void Dispose()
        {
        }
    }

    public class LaserEffectState : EffectState
    {
        public LaserRoot Root => AttachedObject.Object as LaserRoot;
        public float EffectPosition = 0.0f;
        public int References = 0;

        public void Update(double currentTime)
        {
            EffectPosition = Root.Sample(currentTime);
            if(Root.IsExtended)
            {
                EffectPosition += 0.5f;
                EffectPosition *= 0.5f;
            }
            if(Root.Index == 1)
            {
                EffectPosition = 1.0f - EffectPosition;
            }
        }
    }

    public class EffectController : IDisposable
    {
        private BeatmapPlayback playback;
        private AudioTrack track;
        private Dictionary<LaserRoot, LaserEffectState> laserEffectStates = new Dictionary<LaserRoot, LaserEffectState>();
        private List<Dsp> activeDsps = new List<Dsp>();
        private Dsp laserDsp;

        /// <summary>
        /// Hook up the effect controller to playback events
        /// </summary>
        /// <param name="playback"></param>
        public void Initializer(BeatmapPlayback playback, AudioTrack track)
        {
            Debug.Assert(this.playback == null); // Only do this once, or dispose first
            this.playback = playback;
            this.track = track;
            playback.ObjectActivated += PlaybackOnObjectActivated;
            playback.ObjectDeactivated += PlaybackOnObjectDeactivated;
        }

        public void Update()
        {
            // Remove unreferenced laser effects
            var keys = laserEffectStates.Keys.ToArray();
            foreach(var key in keys)
            {
                if(laserEffectStates[key].References == 0)
                    laserEffectStates.Remove(key);
            }

            if(laserEffectStates.Count > 0)
            {
                // Update laser effects
                float combinedLaserState = 0.0f;
                foreach(var effectState in laserEffectStates.Values)
                {
                    effectState.Update(playback.Position);
                    combinedLaserState += effectState.EffectPosition;
                }

                combinedLaserState /= laserEffectStates.Count;
                SetOrUpdateLaserDsp(combinedLaserState);
            }
            else
            {
                RemoveLaserDsp();
            }
        }

        private void RemoveLaserDsp()
        {
            // Remove laser effect
            if(laserDsp != null)
            {
                track.RemoveDsp(laserDsp);
                laserDsp = null;
            }
        }

        private void SetOrUpdateLaserDsp(float value)
        {
            if(laserDsp == null)
            {
                laserDsp = new BiQuadFilter();
                track.AddDsp(laserDsp);
            }
            var bqf = laserDsp as BiQuadFilter;
            bqf.SetPeaking(2.0f, value * 1000.0f + 20.0f, 16.0f, track.SampleRate);
        }

        public void Dispose()
        {
            if(playback != null)
            {
                playback.ObjectActivated -= PlaybackOnObjectActivated;
                playback.ObjectDeactivated -= PlaybackOnObjectDeactivated;
                playback = null;
            }
            
            if(track != null)
            {
                foreach(var dsp in activeDsps)
                    track.RemoveDsp(dsp);

                activeDsps.Clear();
                track = null;
            }
        }

        private void AddLaserEffect(LaserRoot root, ObjectReference obj)
        {
            Debug.Assert(root != null && !laserEffectStates.ContainsKey(root));

            LaserEffectState newState = new LaserEffectState { AttachedObject = obj };
            laserEffectStates.Add(root, newState);
            newState.References = 1;
        }

        private void RemoveLaserEffect(EffectState effect)
        {
            var root = effect.AttachedObject.Object as LaserRoot;
            Debug.Assert(root != null && laserEffectStates.ContainsKey(root));
            
            effect.Dispose();
            laserEffectStates.Remove(root);
        }

        private void PlaybackOnObjectActivated(ObjectReference objectReference)
        {
            var laser = objectReference.Object as Laser;
            if(laser != null)
            {
                var root = laser.Root.Object as LaserRoot;
                LaserEffectState effect;
                if(!laserEffectStates.TryGetValue(root, out effect))
                {
                    AddLaserEffect(root, laser.Root);
                }
                else
                {
                    effect.References++;
                }
            }
        }

        private void PlaybackOnObjectDeactivated(ObjectReference objectReference)
        {
            var laser = objectReference.Object as Laser;
            if(laser != null)
            {
                var root = laser.Root.Object as LaserRoot;
                LaserEffectState effect;
                if(laserEffectStates.TryGetValue(root, out effect))
                {
                    effect.References--;
                }
            }
        }
    }
}