// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using FX2.Game.Beatmap;
using osu.Framework.Graphics;

namespace FX2.Game.Graphics
{
    public class LaserRenderData2D : RenderData2D
    {
        private ObjectReference laserReference;

        public LaserRenderData2D(TrackRenderer2D renderer, ObjectReference laserReference)
            : base(renderer)
        {
            this.laserReference = laserReference;
            CreateDrawable();
        }

        public override void Update(float newPosition)
        {
            if(Drawable != null)
            {
                var pos = Drawable.Position;
                pos.Y = (float)(1.0f - newPosition / Renderer.PlayfieldHeight); // Map to 0-1
                Drawable.Position = pos;
            }
        }

        public override void UpdateViewDuration()
        {
            base.UpdateViewDuration();
            CreateDrawable();
        }

        private void CreateDrawable()
        {
            Dispose();
            
            var laser = this.laserReference.Object as Laser;
            if(laser.Next == null)
                return;

            var laserRoot = laser.Root.Object as LaserRoot;
            var nextLaser = laser.Next?.Object as Laser;

            float x0 = laser.HorizontalPosition;
            float x1 = nextLaser.HorizontalPosition;
            double duration = laser.Next.AbsolutePosition - laserReference.AbsolutePosition;
            Drawable = new LaserDrawable(Renderer, laserReference, ConvertLaserPosition(x0), ConvertLaserPosition(x1),
                (float)(duration / Renderer.ViewDuration))
            {
                Texture = Renderer.LaserTexture,
                Colour = Renderer.LaserColors[laserRoot.Index],
                Alpha = 0.7f,
                BlendingMode = BlendingMode.Additive,
            };

            Renderer.PlayFieldContainer.Add(Drawable);
        }

        private float ConvertLaserPosition(float inPosition)
        {
            return Renderer.LaserWidth * 0.5f + inPosition * (Renderer.PlayfieldWidth - Renderer.LaserWidth);
        }
    }
}