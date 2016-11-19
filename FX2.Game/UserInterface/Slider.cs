// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using FX2.Shared;
using osu.Framework;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Input;
using OpenTK;
using OpenTK.Graphics;

namespace FX2.Game.UserInterface
{
    public class Slider : Container, IStateful<double>
    {
        public Color4 ForegroundColor = Color4.Green;
        public Color4 BackgroundColor = Color4.Gray;

        public double Min;
        public double Max;

        /// <summary>
        /// Snap used when ctrl is held
        /// </summary>
        public double Snap = 1.0;

        /// <summary>
        /// Snap used when alt is held
        /// </summary>
        public double FineSnap = 0.1;

        /// <summary>
        /// Snap used when alt and control are held
        /// </summary>
        public double SuperFineSnap = 0.01;

        private bool dragging;
        private bool hovered;
        private float dragOffset;
        private Box background;
        private Box foreground;
        private double value;
        
        public Func<double> Get;
        public Action<double> Set;

        public double State
        {
            get
            {
                return value;
            }
            set
            {
                value = MathHelper.Clamp(value, Min, Max);
                Set?.Invoke(value);
                UpdateState();
            }
        }

        public double NormalizedValue
        {
            get { return ((value - Min) / (Max - Min)); }
            set
            {
                State = value * (Max - Min) + Min;
            }
        }

        public event EventHandler ValueChanged;

        public Slider()
        {
            Add(new Drawable[]
            {
                background = new Box
                {
                    Colour = BackgroundColor,
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(1.0f)
                },
                foreground = new Box
                {
                    Depth = 1.0f,
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2((float)NormalizedValue, 1.0f),
                    Colour = ForegroundColor,
                },
            });
        }

        private void UpdateState()
        {
            var newValue = Get?.Invoke() ?? Min;
            if(newValue != value)
            {
                value = newValue;
                ValueChanged?.Invoke(this, null);
            }
            UpdateGraphics();
        }

        void UpdateGraphics()
        {
            if(foreground != null)
            {
                foreground.Size = new Vector2((float)NormalizedValue, 1.0f);
            }
        }

        protected override bool OnHover(InputState state)
        {
            hovered = true;
            FadeIn();
            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            if(!dragging) FadeOut();
            hovered = false;
            base.OnHoverLost(state);
        }

        protected override bool OnDragStart(InputState state)
        {
            dragging = true;
            dragOffset = state.Mouse.Position.X;
            return true;
        }

        protected override bool OnDragEnd(InputState state)
        {
            dragging = false;
            if(!hovered) FadeOut();
            return base.OnDragEnd(state);
        }

        protected override bool OnDrag(InputState state)
        {
            base.OnDrag(state);

            double delta = state.Mouse.Delta.X;
            if(state.Keyboard.AltPressed)
            {
                if(state.Keyboard.ControlPressed)
                    SetSnapped(State + delta * SuperFineSnap, SuperFineSnap);
                else
                    SetSnapped(State + delta * FineSnap, FineSnap);
            }
            else if(state.Keyboard.ControlPressed)
            {
                SetSnapped(State + delta * Snap, Snap);
            }
            else
            {
                double offset = state.Mouse.Position.X - Margin.Left;
                var newValue = offset / DrawSize.X;
                NormalizedValue = MathHelper.Clamp(newValue, 0.0, 1.0);
            }
            return true;
        }

        protected override void Update()
        {
            UpdateState();
        }

        private void SetSnapped(double target, double snap)
        {
            State = (int)Math.Round(target / snap) * snap;
        }

        private void FadeIn()
        {
            foreground.FadeColour(ForegroundColor.Scale(1.5f), 200, EasingTypes.Out);
        }

        private void FadeOut()
        {
            foreground.FadeColour(ForegroundColor, 200, EasingTypes.Out);
        }
    }
}