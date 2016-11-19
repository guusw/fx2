// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System;
using System.Collections.Generic;
using System.Diagnostics;
using FX2.Game.Beatmap;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using OpenTK;
using OpenTK.Graphics;

namespace FX2.Game.Graphics
{
    /// <summary>
    /// Draws a single laser segment
    /// </summary>
    public class LaserDrawable : Drawable
    {
        public bool WrapTexture = false;

        public const int MAX_EDGE_SMOOTHNESS = 2;

        /// <summary>
        /// Determines over how many pixels of width the border of the sprite is smoothed
        /// in X and Y direction respectively.
        /// </summary>
        public Vector2 EdgeSmoothness = Vector2.Zero;

        protected Shader TextureShader;
        protected Shader RoundedTextureShader;

        private Texture texture;
        protected Vector2 InflationAmount;

        private List<LaserDrawPart> parts = new List<LaserDrawPart>();
        
        /// <summary>
        /// Creates a new laser drawable
        /// </summary>
        /// <param name="xStart">Relative X starting position</param>
        /// <param name="xEnd">Relative X ending position</param>
        /// <param name="length">Relative Y length</param>
        public LaserDrawable(TrackRenderer2D renderer, ObjectReference laserReference, float xStart, float xEnd, float length)
        {
            RelativeSizeAxes = Axes.Both;
            RelativePositionAxes = Axes.Both;
            Origin = Anchor.BottomLeft;
            Size = new Vector2(1, length);

            var laser = laserReference.Object as Laser;

            Quad quad;

            // Create segment local coordinate system
            Vector2 right =  new Vector2(1.0f, 0.0f);
            Vector2 up = new Vector2(xEnd-xStart, -1.0f);
            Vector2 start = new Vector2(xStart, 1.0f);
            
            if(Math.Abs(length) < 0.001f) // Slam
            {
                float sign = Math.Sign(xEnd - xStart);

                start = new Vector2(xStart - sign * renderer.LaserWidth * 0.5f, 0.5f);
                Size = new Vector2(1, 0.05f); // TODO: Calculate correct slam length/duration
                up = new Vector2(xEnd - xStart + sign * renderer.LaserWidth, 0.0f);
                right = new Vector2(0.0f, sign);

                quad = new Quad(start + up - right * 0.5f,
                    start + up + right * 0.5f,
                    start - right * 0.5f,
                    start + right * 0.5f);
            }
            else
            {
                // Move start up a bit if previous laser was a slam
                if(laser.Previous != null)
                {
                    var previous = laser.Previous.Object as Laser;
                    if(previous.IsInstant())
                    {
                        // TODO: Calculate correct slam length/duration
                        float skip = 0.05f / length;
                        up.Y = up.Y + skip;
                        start.Y -= skip;
                    }
                }

                float halfLaserWidth = renderer.LaserWidth * 0.5f;
                quad = new Quad(start + up - right * halfLaserWidth,
                    start + up + right * halfLaserWidth,
                    start - right * halfLaserWidth,
                    start + right * halfLaserWidth);
            }

            parts.Add(new LaserDrawPart
            {
                Quad = quad,
                TextureRectangle = new RectangleF(0,0, 1.0f, 1.0f)
            });
        }
        
        public bool CanDisposeTexture { get; protected set; }
        
        public Texture Texture
        {
            get { return texture; }
            set
            {
                if(value == texture)
                    return;

                if(texture != null && CanDisposeTexture)
                    texture.Dispose();

                texture = value;
                Invalidate(Invalidation.DrawNode);

                if(Size == Vector2.Zero)
                    Size = new Vector2(texture?.DisplayWidth ?? 0, texture?.DisplayHeight ?? 0);
            }
        }

        protected override DrawNode CreateDrawNode() => new LaserDrawNode();

        public override Drawable Clone()
        {
            LaserDrawable clone = (LaserDrawable)base.Clone();
            clone.texture = texture;

            return clone;
        }

        protected override void ApplyDrawNode(DrawNode node)
        {
            base.ApplyDrawNode(node);

            var scalingMatrix = Matrix3.CreateScale(DrawSize.X, DrawSize.Y, 1.0f);
            foreach(var part in parts)
            {
                part.ScreenSpaceQuad = part.Quad * scalingMatrix * DrawInfo.Matrix;
            }

            LaserDrawNode n = node as LaserDrawNode;

            n.ScreenSpaceDrawQuad = ScreenSpaceDrawQuad;
            n.DrawRectangle = DrawRectangle;
            n.Texture = Texture.WhitePixel;
            n.WrapTexture = WrapTexture;
            n.Parts = parts;

            n.DrawInfo = DrawInfo;
            n.InvalidationID = 0;
            n.TextureShader = TextureShader;
            n.RoundedTextureShader = RoundedTextureShader;
            n.InflationAmount = InflationAmount;
        }

        protected override bool CheckForcedPixelSnapping(Quad screenSpaceQuad)
        {
            return
                Rotation == 0
                && Math.Abs(screenSpaceQuad.Width - Math.Round(screenSpaceQuad.Width)) < 0.1f
                && Math.Abs(screenSpaceQuad.Height - Math.Round(screenSpaceQuad.Height)) < 0.1f;
        }

        protected override Quad ComputeScreenSpaceDrawQuad()
        {
            if(EdgeSmoothness == Vector2.Zero)
            {
                InflationAmount = Vector2.Zero;
                return base.ComputeScreenSpaceDrawQuad();
            }
            else
            {
                Debug.Assert(
                    EdgeSmoothness.X <= MAX_EDGE_SMOOTHNESS &&
                    EdgeSmoothness.Y <= MAX_EDGE_SMOOTHNESS,
                    $@"May not smooth more than {MAX_EDGE_SMOOTHNESS} or will leak neighboring textures in atlas.");

                Vector3 scale = DrawInfo.MatrixInverse.ExtractScale();

                InflationAmount = new Vector2(scale.X * EdgeSmoothness.X, scale.Y * EdgeSmoothness.Y);
                return ToScreenSpace(DrawRectangle.Inflate(InflationAmount));
            }
        }

        [BackgroundDependencyLoader]
        private void load(ShaderManager shaders)
        {
            if(TextureShader == null)
                TextureShader = shaders?.Load(new ShaderDescriptor(VertexShaderDescriptor.Texture2D, FragmentShaderDescriptor.Texture));

            if(RoundedTextureShader == null)
                RoundedTextureShader = shaders?.Load(new ShaderDescriptor(VertexShaderDescriptor.Texture2D, FragmentShaderDescriptor.TextureRounded));
        }
        
    }
}