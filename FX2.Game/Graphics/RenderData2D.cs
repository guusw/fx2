// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using OpenTK;

namespace FX2.Game.Graphics
{
    public class RenderData2D : TrackRendererData
    {
        public bool AspectCorrectHeight = true;

        protected TrackRenderer2D Renderer;
        protected Drawable Drawable;

        public RenderData2D(TrackRenderer2D renderer, Drawable drawable)
        {
            this.Renderer = renderer;
            this.Drawable = drawable;
            renderer.PlayFieldContainer.Add(drawable);
        }

        public RenderData2D(TrackRenderer2D renderer)
        {
            this.Renderer = renderer;
        }

        public override void Update(float newPosition)
        {
            var box = Drawable as Box;

            if(box != null && AspectCorrectHeight)
            {
                var tex = box.Texture;
                float r = (float)tex.Height / tex.Width;
                var parentSize = Drawable.Parent?.DrawSize ?? Vector2.Zero;
                var size = Drawable.Size;
                size.Y = parentSize.X * size.X * r;
                Drawable.Size = size;
            }

            var pos = Drawable.Position;
            pos.Y = (float)(1.0f - newPosition / Renderer.PlayfieldHeight); // Map to 0-1
            Drawable.Position = pos;
        }

        public override void Dispose()
        {
            if(Drawable != null)
            {
                Renderer.PlayFieldContainer.Remove(Drawable, true);
                Drawable = null;
            }
        }
    }
}