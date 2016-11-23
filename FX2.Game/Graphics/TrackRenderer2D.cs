// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using FX2.Game.Beatmap;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using OpenTK;

namespace FX2.Game.Graphics
{
    /// <summary>
    /// A 2D track renderer, will be used for editor eventually. 
    /// Uses the osu-framework drawable system
    /// </summary>
    public class TrackRenderer2D : TrackRendererBase
    {
        public Container PlayFieldContainer;

        public TrackRenderer2D(Container playFieldContainer)
        {
            this.PlayFieldContainer = playFieldContainer;
        }

        protected override TrackRendererData CreateObjectData(ObjectReference obj)
        {
            var button = obj.Object as Button;
            if(button != null)
            {
                var pos = GetButtonPlacement(button.Index);
                var size = new Vector2(GetButtonWidth(button.Index) / PlayfieldWidth, 0);
                var texture = button.Index < 4 ? ButtonTexture : FXButtonTexture;
                return new RenderData2D(this, new Box
                {
                    Texture = texture,
                    Origin = Anchor.BottomLeft,
                    RelativePositionAxes = Axes.Both,
                    Position = pos,
                    RelativeSizeAxes = Axes.X,
                    Size = size,
                    Depth = 4.0f,
                    EdgeSmoothness = Vector2.Zero,
                });
            }

            var hold = obj.Object as Hold;
            if(hold != null)
            {
                var pos = GetButtonPlacement(hold.Index);
                double duration = hold.GetAbsoluteDuration(obj.Measure);
                double length = duration / ViewDuration;
                var size = new Vector2(GetButtonWidth(hold.Index) / PlayfieldWidth, (float)length);
                var texture = hold.Index < 4 ? HoldButtonTexture : FXHoldButtonTexture;
                return new HoldRenderData2D(this, new Box
                {
                    Texture = texture,
                    Origin = Anchor.BottomLeft,
                    RelativePositionAxes = Axes.Both,
                    Position = pos,
                    RelativeSizeAxes = Axes.Both,
                    Size = size,
                    EdgeSmoothness = Vector2.Zero,
                    Depth = (hold.Index < 4) ? 2.0f : 1.0f,
                }, duration);
            }

            var laser = obj.Object as Laser;
            if(laser != null)
            {
                return new LaserRenderData2D(this, obj);
            }

            return null;
        }

        protected override TrackRendererData CreateSplitData(TimeDivisionReference tdr)
        {
            return new RenderData2D(this, new Box
            {
                Texture = SplitTexture,
                Origin = Anchor.BottomCentre,
                RelativePositionAxes = Axes.Both,
                Position = new Vector2(0.5f, 0.0f),
                RelativeSizeAxes = Axes.X,
                Size = new Vector2(1,0),
                EdgeSmoothness = Vector2.Zero,
                Depth = -2.0f,
                Alpha = (tdr.Position.DivisionIndex == 0) ? 1.0f : 0.5f
            });
        }
    }
}