// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using osu.Framework.Graphics;

namespace FX2.Game.Graphics
{
    public class HoldRenderData2D : RenderData2D
    {
        private double duration;

        public HoldRenderData2D(TrackRenderer2D renderer, Drawable drawable, double duration)
            : base(renderer, drawable)
        {
            AspectCorrectHeight = false;
            this.duration = duration;
            UpdateViewDuration();
        }

        public override void UpdateViewDuration()
        {
            var size = Drawable.Size;
            size.Y = (float)(duration / Renderer.ViewDuration);
            Drawable.Size = size;
        }
    }
}