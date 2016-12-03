// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using OpenTK;

namespace FX2.Game.Graphics
{
    public class GameRenderView : Container
    {
        public TrackRenderer2D renderer;
        private BaseGame game;
        private Container playfieldContainer;
        private Box trackBase;

        [BackgroundDependencyLoader]
        private void Load(BaseGame game)
        {
            this.game = game;

            trackBase = new Box
            {
                Depth = 10.0f,
                Origin = Anchor.BottomCentre,
                RelativePositionAxes = Axes.Both,
                Position = new Vector2(0.5f, 1.0f),
                WrapTexture = true,
                EdgeSmoothness = Vector2.Zero,
            };
            Add(trackBase);

            Add(playfieldContainer = new Container
            {
                Origin = Anchor.BottomCentre,
                RelativePositionAxes = Axes.Both,
                Position = new Vector2(0.5f, 1.0f),
            });

            renderer = new TrackRenderer2D(playfieldContainer);
            renderer.Load(game);

            trackBase.Texture = renderer.TrackTexture;
        }

        /*/// <summary>
        /// Updates a split segment to be shown
        /// </summary>
        /// <param name="tdr"></param>
        public void UpdateSplit(TimeDivisionReference tdr)
        {
            Drawable existingDrawable;
            if(!splitObjects.TryGetValue(tdr, out existingDrawable))
            {
                var split = new Box
                {
                    Texture = game.Textures.Get("tick.png"),
                    Origin = Anchor.BottomCentre,
                    RelativePositionAxes = Axes.Both,
                    Position = new Vector2(0.5f, 0.0f),
                    RelativeSizeAxes = Axes.X,
                    EdgeSmoothness = Vector2.Zero,
                };
                split.Size = new Vector2(1.0f, split.Texture.Size.Y);
                existingDrawable = split;
                playfieldContainer.Add(split);
                
                splitObjects.Add(tdr, split);
            }
            else
            {
                pendingSplitRemovals.Remove(tdr);
            }

            UpdateYPosition(tdr.AbsolutePosition, existingDrawable);
        }

        /// <summary>
        /// Updates a split segment to be shown
        /// </summary>
        /// <param name="tdr"></param>
        public void UpdateObject(ObjectReference obj)
        {
            Drawable existingDrawable;
            if(!objects.TryGetValue(obj, out existingDrawable))
            {
                existingDrawable = CreateObjectDrawable(obj);
                if(existingDrawable == null)
                    return;
                playfieldContainer.Add(existingDrawable);
                objects.Add(obj, existingDrawable);
            }
            else
            {
                pendingObjectRemovals.Remove(obj);
            }

            UpdateObject(obj, existingDrawable);
        }
        
        public Drawable CreateObjectDrawable(ObjectReference obj)
        {
            var button = obj.Object as Button;
            if(button != null)
            {
                float buttonSpace = 690 / trackBackground.Texture.Size.X;
                float buttonOffset = (1.0f - buttonSpace) * 0.5f;

                var objectDrawable = new Box
                {
                    Origin = Anchor.BottomLeft,
                    RelativePositionAxes = Axes.Both,
                    Position = new Vector2(0.5f, 0.0f),
                    RelativeSizeAxes = Axes.X
                };

                if(button.Index < 4)
                {
                    objectDrawable.Texture = game.Textures.Get("button.png");
                    float xSize = (1.0f / 4.0f) * buttonSpace;
                    float xPosition = buttonOffset + xSize * button.Index;
                    objectDrawable.Size = new Vector2(xSize, objectDrawable.Texture.Size.Y);
                    objectDrawable.Position = new Vector2(xPosition, 0.0f);
                }
                else
                {
                    objectDrawable.Texture = game.Textures.Get("fxbutton.png");
                    float xSize = (1.0f / 2.0f) * buttonSpace;
                    float xPosition = buttonOffset + xSize * (button.Index - 4);
                    objectDrawable.Size = new Vector2(xSize, objectDrawable.Texture.Size.Y);
                    objectDrawable.Position = new Vector2(xPosition, 0.0f);
                }
                return objectDrawable;
            }

            return null;
        }
        */

        protected override void Update()
        {
            base.Update();
            var parentSize = Parent.DrawSize;

            renderer.Update();

            float heightScale = parentSize.Y / renderer.PlayfieldHeight;
            playfieldContainer.Size = new Vector2(renderer.PlayfieldWidth * heightScale, parentSize.Y);
            trackBase.Size = new Vector2(renderer.TrackWidth * heightScale, parentSize.Y);
        }
    }
}