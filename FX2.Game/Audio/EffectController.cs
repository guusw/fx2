// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FX2.Game.Beatmap;
using FX2.Game.Beatmap.Effects;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Audio.Track;

namespace FX2.Game.Audio
{
    public class EffectBinding : IDisposable
    {
        protected EffectState EffectState { get; private set; }
        protected EffectSettings EffectSettings { get; private set; }
        public PlaybackContext Context { get; private set; }

        public EffectBinding(PlaybackContext playbackContext)
        {
            Context = playbackContext;
        }

        public void CreateEffect(EffectSettings effectSettings)
        {
            if(EffectState != null) throw new InvalidOperationException("CreateEffect called twice");
            EffectSettings = effectSettings;
            EffectState = EffectSettings.CreateEffectState(Context);
        }

        public virtual void Dispose()
        {
            EffectState?.Dispose();
        }

        public virtual void Update(TimeSpan span)
        {
            EffectState?.Update(span);
        }
    }

    public class LaserEffectBinding : EffectBinding
    {
        /// <summary>
        /// Current effect position of this laser
        /// </summary>
        public float EffectPosition = 0.0f;

        public readonly List<ObjectReference> AttachedLasers = new List<ObjectReference>();

        public IEnumerable<LaserRoot> LaserRoots
        {
            get { return AttachedLasers.Select(x => (x.Object as Laser).Root.Object as LaserRoot); }
        }

        public LaserEffectBinding(PlaybackContext playbackContext, EffectSettings effectSettings) : base(playbackContext)
        {
            CreateEffect(effectSettings);
        }

        public override void Update(TimeSpan span)
        {
            base.Update(span);

            // Add all lasers together
            EffectPosition = 0.0f;
            int numLasers = 0;
            HashSet<LaserRoot> processedRoots = new HashSet<LaserRoot>();
            foreach(var laser in LaserRoots)
            {
                // Only process each root once
                if(processedRoots.Contains(laser))
                    continue;

                processedRoots.Add(laser);

                float currentEffectPosition = laser.Sample(Context.Playback.Position);
                if(laser.IsExtended)
                {
                    currentEffectPosition += 0.5f;
                    currentEffectPosition *= 0.5f;
                }
                if(laser.Index == 1)
                {
                    currentEffectPosition = 1.0f - currentEffectPosition;
                }
                EffectPosition += currentEffectPosition;
                numLasers++;
            }

            if(numLasers == 0)
                EffectPosition = 0.0f;
            else
                EffectPosition /= numLasers;

            // Modulate effect
            EffectState.Modulate(EffectPosition);
        }
    }

    public class HoldEffectBinding : EffectBinding
    {
        public ObjectReference AttachedObject { get; private set; }
        public Hold Hold => AttachedObject.Object as Hold;

        public HoldEffectBinding(PlaybackContext playbackContext, ObjectReference attachedObject) : base(playbackContext)
        {
            AttachedObject = attachedObject;
            CreateEffect(playbackContext.Playback.Beatmap.GetEffectSettings(Hold.EffectType));

            // Apply object parameters to hold note effect
            EffectState.ApplyObjectParameters(attachedObject);
        }
        
        public override void Dispose()
        {
            EffectState.Dispose();
        }
    }

    public class EffectController : IDisposable
    {
        private BeatmapPlayback playback;
        private AudioTrack track;

        private LaserEffectBinding laserEffectBinding;
        private Dictionary<Hold, HoldEffectBinding> holdEffectBindings = new Dictionary<Hold, HoldEffectBinding>();

        private Beatmap.Beatmap beatmap;
        private PlaybackContext context;
        private EffectType currentLaserEffectType = EffectType.PeakingFilter;
        private double lastPosition = 0.0;

        private AudioSample laserSlamSample;
        private SampleManager sampleManager;

        /// <summary>
        /// Hook up the effect controller to playback events
        /// </summary>
        /// <param name="playback"></param>
        public void Initializer(BeatmapPlayback playback, AudioTrack track, SampleManager sampleManager)
        {
            Debug.Assert(this.playback == null); // Only do this once, or dispose first
            this.sampleManager = sampleManager;
            this.playback = playback;
            this.track = track;
            playback.ObjectActivated += PlaybackOnObjectActivated;
            playback.ObjectDeactivated += PlaybackOnObjectDeactivated;
            beatmap = playback.Beatmap;
            
            laserSlamSample = sampleManager.Get("laser_slam0.wav");

            // TODO: Pass in to this function as PlaybackContext
            context = new PlaybackContext
            {
                Playback = playback,
                Track = track,
            };
        }

        public void Update()
        {
            double position = playback.Position;
            TimeSpan delta =TimeSpan.FromSeconds(position-lastPosition);
            lastPosition = position;

            foreach(var effect in holdEffectBindings.Values)
                effect.Update(delta);
            laserEffectBinding?.Update(delta);
        }
        
        public void Dispose()
        {
            if(playback != null)
            {
                playback.ObjectActivated -= PlaybackOnObjectActivated;
                playback.ObjectDeactivated -= PlaybackOnObjectDeactivated;
                playback = null;
            }

            // Dispose effects
            laserEffectBinding?.Dispose();
            foreach(var holdEffect in holdEffectBindings.Values)
                holdEffect.Dispose();
        }

        private void AddLaserEffect(ObjectReference obj)
        {
            // Get or create laser effect
            if(laserEffectBinding == null)
            {
                var effectSettings = beatmap.GetEffectSettings(currentLaserEffectType);
                laserEffectBinding = new LaserEffectBinding(context, effectSettings);
            }

            laserEffectBinding.AttachedLasers.Add(obj);
        }

        private void RemoveLaserEffect(ObjectReference obj)
        {
            var laser = obj.Object as Laser;

            laserEffectBinding.AttachedLasers.Remove(obj);
            if(laserEffectBinding.AttachedLasers.Count == 0)
            {
                laserEffectBinding.Dispose();
                laserEffectBinding = null;
            }
        }

        private void TriggerSlam(Laser laser, ObjectReference obj)
        {
            laserSlamSample.Play(true);
        }

        private void PlaybackOnObjectActivated(ObjectReference objectReference)
        {
            var laser = objectReference.Object as Laser;
            if(laser != null)
            {
                if(laser.IsInstant)
                    TriggerSlam(laser, objectReference);
                else
                    AddLaserEffect(objectReference);
                return;
            }

            var hold = objectReference.Object as Hold;
            if(hold != null)
            {
                if(hold.EffectType != EffectType.None)
                    holdEffectBindings.Add(hold, new HoldEffectBinding(context, objectReference));
            }
        }

        private void PlaybackOnObjectDeactivated(ObjectReference objectReference)
        {
            var laser = objectReference.Object as Laser;
            if(laser != null)
            {
                if(!laser.IsInstant)
                    RemoveLaserEffect(objectReference);
                return;
            }

            var hold = objectReference.Object as Hold;
            if(hold != null)
            {
                if(hold.EffectType != EffectType.None)
                {
                    var effectBinding = holdEffectBindings[hold];
                    effectBinding.Dispose();
                    holdEffectBindings.Remove(hold);
                }
            }
        }
    }
}