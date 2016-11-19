// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using osu.Framework.Graphics;
using osu.Framework.Graphics.Batches;
using osu.Framework.Graphics.OpenGL;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace FX2.Game.Graphics
{
    public class HoldDrawNode : SpriteDrawNode
    {
        public Shader HoldShader;
        public override void Draw(IVertexBatch vertexBatch)
        {
            base.Draw(vertexBatch);
        }
    }
}