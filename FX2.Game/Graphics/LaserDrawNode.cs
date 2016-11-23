// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Batches;
using osu.Framework.Graphics.OpenGL;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Textures;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace FX2.Game.Graphics
{
    public class LaserDrawPart
    {
        public Quad Quad;
        public Quad ScreenSpaceQuad;
        public RectangleF TextureRectangle;
    }

    public class LaserDrawNode : DrawNode
    {
        public List<LaserDrawPart> Parts;
        public Color4 Colour;

        public Texture Texture;
        public Quad ScreenSpaceDrawQuad;
        public RectangleF DrawRectangle;
        public Vector2 InflationAmount;
        public bool WrapTexture;

        public Shader TextureShader;
        public Shader RoundedTextureShader;

        private bool NeedsRoundedShader => GLWrapper.IsMaskingActive || InflationAmount != Vector2.Zero;
        
        public override void Draw(IVertexBatch vertexBatch)
        {
            base.Draw(vertexBatch);

            if(Texture == null || Texture.IsDisposed)
                return;

            Shader shader = TextureShader;

            if(InflationAmount != Vector2.Zero)
            {
                // The shader currently cannot deal with negative width and height.
                RectangleF drawRect = DrawRectangle.WithPositiveExtent;
                RoundedTextureShader.GetUniform<Vector4>(@"g_DrawingRect").Value = new Vector4(
                    drawRect.Left,
                    drawRect.Top,
                    drawRect.Right,
                    drawRect.Bottom);

                RoundedTextureShader.GetUniform<Matrix3>(@"g_ToDrawingSpace").Value = DrawInfo.MatrixInverse;
                RoundedTextureShader.GetUniform<Vector2>(@"g_DrawingBlendRange").Value = InflationAmount;
            }

            shader.Bind();

            Texture.TextureGL.WrapMode = WrapTexture ? TextureWrapMode.Repeat : TextureWrapMode.ClampToEdge;

            // TODO: Draw
            foreach(var part in Parts)
            {
                Texture.Draw(part.ScreenSpaceQuad, DrawInfo.Colour, null, vertexBatch as VertexBatch<TexturedVertex2D>,
                    new Vector2(InflationAmount.X / DrawRectangle.Width, InflationAmount.Y / DrawRectangle.Height));
                //Texture.Draw(part.ScreenSpaceQuad, ColourInfo.SingleColour(Colour), part.TextureRectangle);
            }

            shader.Unbind();

            if(InflationAmount != Vector2.Zero)
                RoundedTextureShader.GetUniform<Vector2>(@"g_DrawingBlendRange").Value = Vector2.Zero;

        }
    }
}