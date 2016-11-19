// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using OpenTK;

namespace FX2.Game.Graphics
{
    class RatioAdjust : Container
    {
        public override bool Contains(Vector2 screenSpacePos) => true;

        public RatioAdjust()
        {
            RelativeSizeAxes = Axes.Both;
        }

        protected override void Update()
        {
            base.Update();
            Vector2 parent = Parent.DrawSize;

            Scale = new Vector2(Math.Min(parent.Y / 768f, parent.X / 1024f));
            Size = new Vector2(1 / Scale.X);
        }
    }
}