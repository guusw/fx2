// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;

namespace FX2.Game.Beatmap.Effects
{
    /// <summary>
    /// The state of an effect when instantiated
    /// </summary>
    public abstract class EffectState : IDisposable
    {
        private bool enabled;

        public virtual void Dispose()
        {
        }

        /// <summary>
        /// Gets or sets if the effect is enabled, basically a mute button
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set
            {
                if(value != enabled)
                {
                    enabled = value;
                    OnEnableToggled();
                }
            }
        }

        public static TEffectState Instantiate<TEffectState>(PlaybackContext playbackContext, EffectSettings settings) where TEffectState : EffectState, new()
        {
            var state = new TEffectState();
            state.Init(playbackContext, settings);
            return state;
        }

        /// <summary>
        /// Initialize the effect for given audio track
        /// </summary>
        public abstract void Init(PlaybackContext playbackContext, EffectSettings settings);

        /// <summary>
        /// Modulate the effect by a single input signal (combined laser output)
        /// </summary>
        public abstract void Modulate(float input);

        /// <summary>
        /// Tries to apply the effect on the given object
        /// </summary>
        public abstract void ApplyObjectParameters(ObjectReference objectReference);

        public virtual void Update(TimeSpan elapsedTime)
        {
        }

        /// <summary>
        /// Called when the enabled state of this effect is toggled
        /// </summary>
        protected virtual void OnEnableToggled()
        {
        }
    }

    /// <summary>
    /// An effect that uses a single Dsp
    /// </summary>
    public abstract class DspEffectState<TDspType> : EffectState where TDspType : Dsp
    {
        public TDspType Dsp { get; private set; }
        public EffectSettings Settings { get; private set; }
        protected PlaybackContext Context { get; private set; }

        public override void Init(PlaybackContext playbackContext, EffectSettings settings)
        {
            Context = playbackContext;
            Settings = settings;
            Dsp = CreateDsp();
            Context.Track.AddDsp(Dsp);
            ApplyInitialSettings();
        }

        protected virtual void ApplyInitialSettings()
        {
        }

        /// <summary>
        /// Should create the dsp for this effect
        /// </summary>
        /// <returns></returns>
        protected abstract TDspType CreateDsp();

        public override void Dispose()
        {
            base.Dispose();
            Context.Track.RemoveDsp(Dsp);
        }
    }

    /// <summary>
    /// A default DspEffectState that just creates a new dsp using the default constructor and performs no modulation
    /// </summary>
    public class DspEffectStateDefault<TDspType> : DspEffectState<TDspType> where TDspType : Dsp, new()
    {
        protected override TDspType CreateDsp() => new TDspType();

        public override void Modulate(float input)
        {
        }
        public override void ApplyObjectParameters(ObjectReference objectReference)
        {
        }
    }
}